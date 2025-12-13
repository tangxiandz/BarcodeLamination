using BarcodeLaminationAPI.Data;
using BarcodeLaminationModel.Models;
using Microsoft.EntityFrameworkCore;

namespace BarcodeLaminationAPI.Services
{
    public class FeedingService : IFeedingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMaterialService _materialService;

        public FeedingService(ApplicationDbContext context, IMaterialService materialService)
        {
            _context = context;
            _materialService = materialService;
        }

        public async Task<FeedingResponse> ProcessFeedingAsync(FeedingRequest request)
        {
            var response = new FeedingResponse();
            var filmCoatingValidations = new List<FilmCoatingValidationResult>();

            try
            {
                // 1. 获取物料信息
                var material = await _materialService.GetMaterialByFabricERPCodeAsync(request.ProductERPCode);
                if (material == null)
                {
                    return new FeedingResponse
                    {
                        Success = false,
                        Message = "未找到对应的物料信息"
                    };
                }

                // 2. 验证刀模
                var moldValidation = new ValidationResult();
                if (material.MoldNumber == request.MoldQRCode)
                {
                    moldValidation.IsValid = true;
                    moldValidation.Message = "刀模验证通过";
                }
                else
                {
                    moldValidation.IsValid = false;
                    moldValidation.Message = "刀模与产品不匹配";
                }

                // 3. 验证覆膜二维码
                int totalQuantity = 0;
                foreach (var filmCoatingQRCode in request.FilmCoatingQRCodes)
                {
                    var validation = await ValidateFilmCoatingQRCode(filmCoatingQRCode, request);
                    filmCoatingValidations.Add(validation);

                    if (validation.IsValid)
                    {
                        totalQuantity += validation.Quantity;
                    }
                }

                // 检查是否全部验证通过
                bool isAllValid = moldValidation.IsValid &&
                                 filmCoatingValidations.All(v => v.IsValid);

                // 创建上料记录
                var record = new FeedingRecord
                {
                    ProductERPCode = material.ProductERPCode,
                    MoldNumber = request.MoldQRCode,
                    FilmCoatingQRCode = material.FabricERPCode, // 保存所有二维码
                    FilmCoatingCount = request.FilmCoatingQRCodes.Count,
                    FilmCoatingQuantity = totalQuantity,
                    Operator = request.Operator,
                    PDADeviceId = request.PDADeviceId,
                    FeedingTime = DateTime.Now,
                    ErrorMessage = isAllValid ? null : "验证失败",
                    IsClosed = 0,
                    BoxId = 0
                };

                _context.FeedingRecords.Add(record);
                await _context.SaveChangesAsync();

                // 创建上料记录明细
                foreach (var validation in filmCoatingValidations)
                {
                    var detail = new FeedingRecordsList
                    {
                        FeedingRecordId = record.Id,
                        FilmCoatingQRCode = validation.QRCode,
                        ERPCode = validation.ERPCode,
                        Quantity = validation.Quantity,
                        BatchNumber = validation.BatchNumber,
                        CreatedTime = DateTime.Now
                    };
                    _context.FeedingRecordsList.Add(detail);
                }
                await _context.SaveChangesAsync();

                response.Success = true;
                response.Message = isAllValid ? "上料成功" : "上料完成，但存在验证失败";
                response.IsAllValid = isAllValid;
                response.Record = record;
                response.MoldValidation = moldValidation;
                response.FilmCoatingValidations = filmCoatingValidations;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"上料失败：{ex.Message}";
            }

            return response;
        }

        private async Task<FilmCoatingValidationResult> ValidateFilmCoatingQRCode(string qrCode, FeedingRequest req)
        {
            var result = new FilmCoatingValidationResult
            {
                QRCode = qrCode,
                Index = 0 // 会在调用处设置
            };

            try
            {
                // 获取物料信息
                var material = await _materialService.GetMaterialByFabricERPCodeAsync(req.ProductERPCode);
                if (material == null)
                {
                    result.IsValid = false;
                    result.Message = "物料不存在";
                    return result;
                }

                string erpCode = null;
                string batchNumber = null;
                int quantity = 0;

                // 方式1: 解析GS1格式 (包含分隔符 \u001D)
                if (qrCode.Contains("ERP") || qrCode.Contains("\u001D"))
                {
                    erpCode = ExtractValueByPrefix(qrCode, "ERP");
                    batchNumber = ExtractValueByPrefix(qrCode, "D");
                    var quantityStr = ExtractValueByPrefix(qrCode, "Q");

                    if (!string.IsNullOrEmpty(quantityStr) && int.TryParse(quantityStr, out int qty))
                    {
                        quantity = qty;
                    }

                    result.ERPCode = erpCode;
                    result.BatchNumber = batchNumber;
                    result.Quantity = quantity;

                    // 验证ERP号是否匹配
                    if (erpCode == material.FabricERPCode)
                    {
                        result.IsValid = true;
                        result.Message = "面料验证通过";
                    }
                    else
                    {
                        result.IsValid = false;
                        result.Message = $"面料ERP不匹配，期望: {material.FabricERPCode}，实际: {erpCode}";
                    }

                    return result;
                }

                result.IsValid = false;
                result.Message = "无法解析二维码格式";
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"解析二维码失败: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 从GS1格式字符串中提取指定前缀的值
        /// 例如: 从 "ERPMH002839\u001DP\u001DQ1000\u001DD151127" 中提取:
        ///   ExtractValueByPrefix(text, "ERP") -> "MH002839"
        ///   ExtractValueByPrefix(text, "Q") -> "1000"
        ///   ExtractValueByPrefix(text, "D") -> "151127"
        /// </summary>
        private string ExtractValueByPrefix(string text, string prefix)
        {
            try
            {
                // 处理GS1分隔符 (ASCII 29, 即 \u001D)
                const char gs1Separator = '\u001D';

                // 如果包含GS1分隔符，先按分隔符拆分
                if (text.Contains(gs1Separator))
                {
                    var parts = text.Split(gs1Separator);
                    foreach (var part in parts)
                    {
                        if (part.StartsWith(prefix))
                        {
                            return part.Substring(prefix.Length);
                        }
                    }
                }

                // 如果没有GS1分隔符，尝试直接查找
                // 使用正则表达式匹配前缀后的内容（直到遇到GS1分隔符、回车或换行）
                var pattern = $"{prefix}([^{gs1Separator}\\r\\n]*)";
                var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // 其他方法保持不变...
        public async Task<PagedResponse<FeedingRecord>> GetFeedingRecordsAsync(QueryRequest request)
        {
            var query = _context.FeedingRecords.AsQueryable();

            if (request.StartDate.HasValue)
                query = query.Where(r => r.FeedingTime >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                query = query.Where(r => r.FeedingTime <= request.EndDate.Value);
            if (!string.IsNullOrEmpty(request.ProductERPCode))
                query = query.Where(r => r.ProductERPCode.Contains(request.ProductERPCode));

            query = query.OrderByDescending(r => r.FeedingTime);

            var totalCount = await query.CountAsync();
            var records = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponse<FeedingRecord>
            {
                Data = records,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<List<FeedingRecord>> GetValidFeedingRecordsAsync()
        {
            return await _context.FeedingRecords
                .Where(r => r.IsClosed.Equals(0)) // 只返回有效且未装箱的记录
                .OrderByDescending(r => r.FeedingTime)
                .ToListAsync();
        }

        // 新增方法：根据ID获取上料记录明细
        public async Task<List<FeedingRecordsList>> GetFeedingRecordDetailsAsync(int feedingRecordId)
        {
            return await _context.FeedingRecordsList
                .Where(d => d.FeedingRecordId == feedingRecordId)
                .ToListAsync();
        }
    }
}