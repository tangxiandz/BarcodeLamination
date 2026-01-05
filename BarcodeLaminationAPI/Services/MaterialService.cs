using Microsoft.EntityFrameworkCore;
using BarcodeLaminationAPI.Data;
using BarcodeLaminationModel.Models;

namespace BarcodeLaminationAPI.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _context;

        public MaterialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<Material>> GetMaterialsAsync(int pageNumber = 1, int pageSize = 20, string searchTerm = "")
        {
            var query = _context.Materials.AsQueryable();

            // 搜索过滤
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(m =>
                    (m.ProductERPCode != null && m.ProductERPCode.Contains(searchTerm)) ||
                    (m.ProductPartDescription != null && m.ProductPartDescription.Contains(searchTerm)) ||
                    (m.MoldNumber != null && m.MoldNumber.Contains(searchTerm)) ||
                    (m.FabricERPCode != null && m.FabricERPCode.Contains(searchTerm)) ||
                    (m.FabricPartDescription != null && m.FabricPartDescription.Contains(searchTerm))
                );
            }

            // 只返回活跃的物料
            query = query.Where(m => m.IsActive);

            // 计算总数
            var totalCount = await query.CountAsync();

            // 分页 - 更安全的排序方式
            var materials = await query
                .OrderBy(m => m.ProductERPCode == null ? "" : m.ProductERPCode)
                .ThenBy(m => m.Id) // 添加次要排序条件
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<Material>
            {
                Data = materials,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<Material>> GetMaterialsAsync()
        {
            var query = _context.Materials.AsQueryable();


            // 只返回活跃的物料
            query = query.Where(m => m.IsActive);

            // 计算总数
            var totalCount = await query.CountAsync();

            // 分页 - 更安全的排序方式
            var materials = await query
                .OrderBy(m => m.ProductERPCode == null ? "" : m.ProductERPCode)
                .ThenBy(m => m.Id) // 添加次要排序条件
                .ToListAsync();

            return materials;
        }

        public async Task<Material> GetMaterialByIdAsync(int id)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        public async Task<Material> GetMaterialByProductERPCodeAsync(string productERPCode)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.ProductERPCode == productERPCode && m.IsActive);
        }
        public async Task<Material> GetMaterialByProductERPCodeAsync(string productERPCode,string FabricERPCode)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.ProductERPCode == productERPCode&&m.FabricERPCode== FabricERPCode && m.IsActive);
        }
        public async Task<Material> GetMaterialByFabricERPCodeAsync(string FabricERPCode)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.FabricERPCode == FabricERPCode && m.IsActive);
        }

        public async Task<Material> GetMaterialByFabricERPCodeAsync(string FabricERPCode,string MoldNumber)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.FabricERPCode == FabricERPCode&&m.MoldNumber== MoldNumber && m.IsActive);
        }

        public async Task<Material> GetMaterialByMoldNumberAsync(string moldNumber)
        {
            return await _context.Materials
                .FirstOrDefaultAsync(m => m.MoldNumber == moldNumber && m.IsActive);
        }

        public async Task<Material> CreateMaterialAsync(MaterialRequest request)
        {
            // 检查ERP号是否已存在
            if (await _context.Materials.AnyAsync(m => m.ProductERPCode == request.ProductERPCode))
            {
                throw new Exception($"成品ERP号 {request.ProductERPCode} 已存在");
            }

            // 检查刀模号是否已存在
            if (await _context.Materials.AnyAsync(m => m.MoldNumber == request.MoldNumber))
            {
                throw new Exception($"刀模号 {request.MoldNumber} 已存在");
            }

            var material = new Material
            {
                ProductERPCode = request.ProductERPCode,
                ProductPartDescription = request.ProductPartDescription,
                MoldNumber = request.MoldNumber,
                PackingQuantity = request.PackingQuantity,
                FabricRollCount = request.FabricRollCount,
                FabricERPCode = request.FabricERPCode,
                FabricPartDescription = request.FabricPartDescription,
                CreateTime = DateTime.Now,
                CreatedBy = request.CreatedBy,
                IsActive = true
            };

            _context.Materials.Add(material);
            await _context.SaveChangesAsync();

            return material;
        }

        public async Task<Material> UpdateMaterialAsync(int id, MaterialRequest request)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                throw new Exception("物料不存在");
            }

            // 检查ERP号是否被其他物料使用
            if (await _context.Materials.AnyAsync(m => m.Id != id && m.ProductERPCode == request.ProductERPCode))
            {
                throw new Exception($"成品ERP号 {request.ProductERPCode} 已被其他物料使用");
            }

            // 检查刀模号是否被其他物料使用
            if (await _context.Materials.AnyAsync(m => m.Id != id && m.MoldNumber == request.MoldNumber))
            {
                throw new Exception($"刀模号 {request.MoldNumber} 已被其他物料使用");
            }

            material.ProductERPCode = request.ProductERPCode;
            material.ProductPartDescription = request.ProductPartDescription;
            material.MoldNumber = request.MoldNumber;
            material.PackingQuantity = request.PackingQuantity;
            material.FabricRollCount = request.FabricRollCount;
            material.FabricERPCode = request.FabricERPCode;
            material.FabricPartDescription = request.FabricPartDescription;

            await _context.SaveChangesAsync();

            return material;
        }

        public async Task<bool> DeleteMaterialAsync(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return false;
            }

            // 软删除：将IsActive设置为false
            material.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }
    }
} 
 