using Microsoft.AspNetCore.Mvc;
using BarcodeLaminationAPI.Services;
using BarcodeLaminationModel.Models;

namespace PDA.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnloadingController : ControllerBase
    {
        private readonly IUnloadingService _unloadingService;

        public UnloadingController(IUnloadingService unloadingService)
        {
            _unloadingService = unloadingService;
        }

        /// <summary>
        /// 下料工序 - 选择上料记录并打印标签
        /// </summary>
        [HttpPost("process")]
        public async Task<ActionResult<UnloadingResponse>> ProcessUnloading([FromBody] UnloadingRequest request)
        {
            try
            {
                if (request.FeedingRecordIds == null || request.FeedingRecordIds.Count == 0)
                {
                    return BadRequest(new { message = "上料记录ID不能为空" });
                }
                // 限制只能选择一条记录
                if (request.FeedingRecordIds.Count > 1)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "打印操作只能选择一条记录进行处理"
                    });
                }
                var result = await _unloadingService.ProcessUnloadingAsync(request);

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
                    message = $"下料处理失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 下料工序 - 下料完成
        /// </summary>
        [HttpPost("complete")]
        public async Task<ActionResult<UnloadingResponse>> ProcessClosed([FromBody] CompleteRequest request)
        {
            try
            {
                if (request.FeedingRecordId == 0)
                {
                    return BadRequest(new { message = "上料记录ID不能为空" });
                }

                var result = await _unloadingService.ProcessClosedAsync(request.FeedingRecordId);

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
                    message = $"下料处理失败: {ex.Message}"
                });
            }
        }

        public class CompleteRequest
        {
            public int FeedingRecordId { get; set; }
        }

        /// <summary>
        /// 查询下料记录
        /// </summary>
        [HttpGet("records")]
        public async Task<ActionResult<PagedResponse<UnloadingRecord>>> GetUnloadingRecords(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? productERPCode,
            [FromQuery] string? batchNumber,
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
                    BatchNumber = batchNumber,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var result = await _unloadingService.GetUnloadingRecordsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"查询下料记录失败: {ex.Message}" });
            }
        }
    }
}