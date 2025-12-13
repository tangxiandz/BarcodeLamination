using BarcodeLaminationAPI.Data;
using BarcodeLaminationModel.Models;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.EntityFrameworkCore;

namespace BarcodeLaminationAPI.Services
{
    public class UnloadingService : IUnloadingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMaterialService _materialService;

        public UnloadingService(ApplicationDbContext context, IMaterialService materialService)
        {
            _context = context;
            _materialService = materialService;
        }

        public async Task<UnloadingResponse> ProcessUnloadingAsync(UnloadingRequest request)
        {
            var response = new UnloadingResponse();

            try
            {
                // 获取选中的上料记录
                var feedingRecords = await _context.FeedingRecords
                    .Where(f => request.FeedingRecordIds.Contains(f.Id) && f.IsClosed.Equals(0))
                    .ToListAsync();

                if (!feedingRecords.Any())
                {
                    response.Success = false;
                    response.Message = "未找到有效的上料记录";
                    return response;
                }
               
                // 获取第一条记录的信息
                var firstRecord = feedingRecords.First();
                string productERPCode = firstRecord.ProductERPCode;

                // 累计数量
                int totalQuantity = feedingRecords.Sum(f => f.FilmCoatingQuantity);

                // 生成批次号（使用当前日期）
                string batchNumber = DateTime.Now.ToString("yyyyMMdd");
                var material = await _materialService.GetMaterialByProductERPCodeAsync(productERPCode);
                // 创建下料记录
                var unloadingRecord = new UnloadingRecord
                {
                    ProductERPCode = productERPCode,
                    ProductPartDescription= material.ProductPartDescription,
                    ProductPartDescription2= material.FabricERPCode,
                    Quantity = totalQuantity,
                    BatchNumber = batchNumber,
                    PrintTime = DateTime.Now,
                    PrintStatus=0,
                    PrintedBy = request.PrintedBy,
                    PCDeviceId = request.PCDeviceId,
                    CreatedTime = DateTime.Now
                };

                _context.UnloadingRecords.Add(unloadingRecord);
                await _context.SaveChangesAsync();

                //// 更新上料记录的装箱状态
                //foreach (var feedingRecord in feedingRecords)
                //{
                //    feedingRecord.IsClosed = 1;
                //    feedingRecord.BoxId = unloadingRecord.Id;
                //}
                //await _context.SaveChangesAsync();

                // 生成新二维码
                string newQRCode = $"{productERPCode}|{batchNumber}|{totalQuantity}";

                response.Success = true;
                response.Message = "下料成功";
                response.Record = unloadingRecord;
                response.NewQRCode = newQRCode;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"下料失败: {ex.Message}";
            }

            return response;
        }

        public async Task<UnloadingResponse> ProcessClosedAsync(int FeedingRecordId)
        {
            var response = new UnloadingResponse();

            try
            {
                // 获取选中的上料记录
                var feedingRecord = await _context.FeedingRecords
                    .FirstOrDefaultAsync(f => FeedingRecordId.Equals(f.Id) && f.IsClosed.Equals(0));

                if (feedingRecord == null)
                {
                    response.Success = false;
                    response.Message = "未找到有效的上料记录";
                    return response;
                }

                feedingRecord.IsClosed = 1;
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = "下料成功";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"下料失败: {ex.Message}";
            }

            return response;
        }

        public async Task<PagedResponse<UnloadingRecord>> GetUnloadingRecordsAsync(QueryRequest request)
        {
            var query = _context.UnloadingRecords.AsQueryable();

            if (request.StartDate.HasValue)
                query = query.Where(r => r.PrintTime >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                query = query.Where(r => r.PrintTime <= request.EndDate.Value);
            if (!string.IsNullOrEmpty(request.ProductERPCode))
                query = query.Where(r => r.ProductERPCode.Contains(request.ProductERPCode));
            if (!string.IsNullOrEmpty(request.BatchNumber))
                query = query.Where(r => r.BatchNumber.Contains(request.BatchNumber));

            query = query.OrderByDescending(r => r.PrintTime);

            var totalCount = await query.CountAsync();
            var records = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<UnloadingRecord>
            {
                Data = records,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}