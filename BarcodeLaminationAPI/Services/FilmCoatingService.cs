
using BarcodeLaminationAPI.Data;
using BarcodeLaminationModel.Models;
using BarcodeLaminationModel.Models.Print;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BarcodeLaminationAPI.Services
{
    public class FilmCoatingService : IFilmCoatingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMaterialService _materialService;
        private readonly ILogger<FilmCoatingService> _logger;
        private readonly IBarTenderPrintService _printService;

        public FilmCoatingService(ApplicationDbContext context, IMaterialService materialService,
             IBarTenderPrintService printService,
        ILogger<FilmCoatingService> logger)
        {
            _context = context;
            _materialService = materialService;
            _printService = printService;
            _logger = logger;
        }

        public async Task<FilmCoatingResponse> ProcessFilmCoatingAsync(FilmCoatingRequest request)
        {
            var response = new FilmCoatingResponse();
            try
            {
                _logger.LogInformation("开始处理覆膜请求");
                // 解析原始二维码（格式：ERP号|数量|批次）
                var qrParts = request.ErpCode;
                if (qrParts.Length < 3)
                {
                    response.Success = false;
                    response.Message = "二维码格式不正确";
                    return response;
                }
                // 获取物料信息
                var material = await _materialService.GetMaterialByProductERPCodeAsync(request.ErpCode);
                if (material == null)
                {
                    response.Success = false;
                    response.Message = $"未找到ERP码为 {request.ErpCode} 的物料信息";
                    return response;
                }

                // 生成新二维码（格式：ERP号|数量|批次）
                var batchNumber = DateTime.Now.ToString("yyyyMMdd");
                var newQRCode = $"{material.ProductERPCode}|{request.Quantity}|{batchNumber}";

                // 创建记录
                var record = new FilmCoatingRecord
                {
                    OriginalERPCode = request.OriginalQRCode,
                    NewERPCode = material.FabricERPCode,
                    ProductERPCode = material.ProductERPCode,
                    Quantity = request.Quantity,
                    BatchNumber = batchNumber,
                    ProductPartDescription = material.ProductPartDescription,
                    PrintedBy = request.PrintedBy,
                    PDADeviceId = request.PDADeviceId,
                    Status= "待打印",
                    PrintTime = DateTime.Now
                };

                _context.FilmCoatingRecords.Add(record);
                await _context.SaveChangesAsync();


                // 6. 准备打印数据
                var printData = new FilmCoatingPrintData
                {
                    ErpCode = material.ProductERPCode,
                    ProductPartDescription = material.ProductPartDescription ?? request.ProductPartDescription,
                    OriginalQuantity = request.Quantity,
                    NewQuantity = request.Quantity,
                    NewBatch= batchNumber,
                    NewQRCode = newQRCode,
                    PrintedBy = record.PrintedBy,
                    Copies = 1,
                    GenerateQRCode = true
                };
                response.Success = true;
                response.Message = "覆膜标签打印成功";
                response.NewQRCode = newQRCode;
                response.ProductPartDescription = material.ProductPartDescription ?? request.ProductPartDescription;
                response.Record = record;
                response.PrintResult = null;

                _logger.LogInformation("覆膜处理成功");
                // 7. 打印标签
                //var printResult = await _printService.PrintFilmCoatingLabel(printData);

                //record.PrintTime = printResult.PrintTime;
                //record.Status = printResult.Success ? "已打印" : "打印失败"+ printResult.Message;

                //await _context.SaveChangesAsync();

                //if (printResult.Success)
                //{
                //    response.Success = true;
                //    response.Message = "覆膜标签打印成功";
                //    response.NewQRCode = newQRCode;
                //    response.ProductPartDescription = material.ProductPartDescription ?? request.ProductPartDescription;
                //    response.Record = record;
                //    response.PrintResult = printResult;

                //    _logger.LogInformation("覆膜处理成功");
                //}
                //else
                //{
                //    response.Success = false;
                //    response.Message = $"保存成功但打印失败: {printResult.Message}";
                //    response.NewQRCode = newQRCode;
                //    response.Record = record;
                //    response.PrintResult = printResult;

                //    _logger.LogWarning("覆膜保存成功但打印失败");
                //}
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"处理失败：{ex.Message}";
            }

            return response;
        }

        public async Task<PagedResponse<FilmCoatingRecord>> GetFilmCoatingRecordsAsync(QueryRequest request)
        {
            var query = _context.FilmCoatingRecords.AsQueryable();

            // 过滤条件
            if (request.StartDate.HasValue)
            {
                query = query.Where(r => r.PrintTime >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                query = query.Where(r => r.PrintTime <= request.EndDate.Value);
            }
            if (!string.IsNullOrEmpty(request.ProductERPCode))
            {
                query = query.Where(r => r.ProductERPCode.Contains(request.ProductERPCode));
            }
            if (!string.IsNullOrEmpty(request.BatchNumber))
            {
                query = query.Where(r => r.BatchNumber.Contains(request.BatchNumber));
            }

            // 排序
            query = query.OrderByDescending(r => r.PrintTime);

            // 分页
            var totalCount = await query.CountAsync();
            var records = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<FilmCoatingRecord>
            {
                Data = records,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
