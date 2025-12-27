using Microsoft.AspNetCore.Mvc;
using BarcodeLaminationAPI.Services;
using BarcodeLaminationModel.Models;

namespace MaterialsController.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilmCoatingController : ControllerBase
    {
        private readonly IFilmCoatingService _filmCoatingService;

        public FilmCoatingController(IFilmCoatingService filmCoatingService)
        {
            _filmCoatingService = filmCoatingService;
        }

        /// <summary>
        /// 覆膜工序 - 扫描二维码并打印新标签
        /// </summary>
        [HttpPost("process")]
        public async Task<ActionResult<FilmCoatingResponse>> ProcessFilmCoating([FromBody] FilmCoatingRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OriginalQRCode))
                {
                    return BadRequest(new { message = "二维码内容不能为空" });
                }
                var result = await _filmCoatingService.ProcessFilmCoatingAsync(request);

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
                    message = $"覆膜处理失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 查询覆膜记录
        /// </summary>
        [HttpGet("records")]
        public async Task<ActionResult<PagedResponse<FilmCoatingRecord>>> GetFilmCoatingRecords(
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

                var result = await _filmCoatingService.GetFilmCoatingRecordsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"查询覆膜记录失败: {ex.Message}" });
            }
        }
    }
}