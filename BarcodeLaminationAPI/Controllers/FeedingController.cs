using Microsoft.AspNetCore.Mvc;
using BarcodeLaminationAPI.Services;
using BarcodeLaminationModel.Models;

namespace PDA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedingController : ControllerBase
    {
        private readonly IFeedingService _feedingService;

        public FeedingController(IFeedingService feedingService)
        {
            _feedingService = feedingService;
        }

        /// <summary>
        /// 上料工序 - 验证刀模和面料并记录上料
        /// </summary>
        [HttpPost("process")]
        public async Task<ActionResult<FeedingResponse>> ProcessFeeding([FromBody] FeedingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ProductERPCode))
                {
                    return BadRequest(new { message = "成品ERP号不能为空" });
                }

                if (string.IsNullOrEmpty(request.MoldQRCode))
                {
                    return BadRequest(new { message = "刀模二维码不能为空" });
                }

                // 修改为使用 FilmCoatingQRCodes
                if (request.FilmCoatingQRCodes == null || request.FilmCoatingQRCodes.Count == 0)
                {
                    return BadRequest(new { message = "覆膜二维码不能为空" });
                }

                var result = await _feedingService.ProcessFeedingAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"上料处理失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 查询上料记录
        /// </summary>
        [HttpGet("records")]
        public async Task<ActionResult<PagedResponse<FeedingRecord>>> GetFeedingRecords(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? productERPCode,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var request = new QueryRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProductERPCode = productERPCode,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _feedingService.GetFeedingRecordsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"查询上料记录失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 获取有效的上料记录（用于下料工序选择）
        /// </summary>
        [HttpGet("valid-records")]
        public async Task<ActionResult<List<FeedingRecord>>> GetValidFeedingRecords()
        {
            try
            {
                var records = await _feedingService.GetValidFeedingRecordsAsync();
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取有效上料记录失败: {ex.Message}" });
            }
        }
    }
}