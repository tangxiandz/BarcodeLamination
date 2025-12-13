using Microsoft.AspNetCore.Mvc;
using BarcodeLaminationAPI.Services;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialsController : ControllerBase
    {
        private readonly IMaterialService _materialService;

        public MaterialsController(IMaterialService materialService)
        {
            _materialService = materialService;
        }
        /// <summary>
        /// 获取所有物料列表（不分页，用于下拉框）
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<List<Material>>> GetAllMaterials()
        {
            try
            {
                // 假设您的 MaterialService 有获取所有物料的方法
                var result = await _materialService.GetMaterialsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料列表失败: {ex.Message}" });
            }
        }
        /// <summary>
        /// 获取物料列表（分页）
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Material>>> GetMaterials(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = "")
        {
            try
            {
                var result = await _materialService.GetMaterialsAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料列表失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 根据ID获取物料信息
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Material>> GetMaterialById(int id)
        {
            try
            {
                var material = await _materialService.GetMaterialByIdAsync(id);
                if (material == null)
                {
                    return NotFound(new { message = "未找到对应的物料信息" });
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料信息失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 根据成品ERP号获取物料信息
        /// </summary>
        [HttpGet("by-product/{productERPCode}")]
        public async Task<ActionResult<Material>> GetMaterialByProductERPCode(string productERPCode)
        {
            try
            {
                var material = await _materialService.GetMaterialByProductERPCodeAsync(productERPCode);
                if (material == null)
                {
                    return NotFound(new { message = "未找到对应的物料信息" });
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料信息失败: {ex.Message}" });
            }
        }

        [HttpGet("by-fabric/{FabricERPCode}")]
        public async Task<ActionResult<Material>> GetMaterialByFabricERPCode(string FabricERPCode)
        {
            try
            {
                var material = await _materialService.GetMaterialByFabricERPCodeAsync(FabricERPCode);
                if (material == null)
                {
                    return NotFound(new { message = "未找到对应的物料信息" });
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料信息失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 根据刀模号获取物料信息
        /// </summary>
        [HttpGet("by-mold/{moldNumber}")]
        public async Task<ActionResult<Material>> GetMaterialByMoldNumber(string moldNumber)
        {
            try
            {
                var material = await _materialService.GetMaterialByMoldNumberAsync(moldNumber);
                if (material == null)
                {
                    return NotFound(new { message = "未找到对应的物料信息" });
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"获取物料信息失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 创建物料
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Material>> CreateMaterial([FromBody] MaterialRequest request)
        {
            try
            {
                var material = await _materialService.CreateMaterialAsync(request);
                return CreatedAtAction(nameof(GetMaterialById), new { id = material.Id }, material);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"创建物料失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 更新物料
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Material>> UpdateMaterial(int id, [FromBody] MaterialRequest request)
        {
            try
            {
                var material = await _materialService.UpdateMaterialAsync(id, request);
                if (material == null)
                {
                    return NotFound(new { message = "物料不存在" });
                }
                return Ok(material);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"更新物料失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 删除物料
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMaterial(int id)
        {
            try
            {
                var result = await _materialService.DeleteMaterialAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "物料不存在" });
                }
                return Ok(new { message = "物料删除成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"删除物料失败: {ex.Message}" });
            }
        }
    }
}