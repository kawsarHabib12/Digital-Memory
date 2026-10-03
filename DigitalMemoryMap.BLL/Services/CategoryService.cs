using System;
using System.Linq;
using DigitalMemoryMap.BLL.DTOs;
using DigitalMemoryMap.BLL.Exceptions;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.DAL.Entities;
using DigitalMemoryMap.DAL.Interfaces;

namespace DigitalMemoryMap.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<CategoryDto>> GetAllActiveAsync()
    {
        var categories = await _categoryRepository.GetAllActiveAsync();
        return categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var existing = await _categoryRepository.GetByNameAsync(dto.Name);
        if (existing != null)
        {
            throw new ConflictException($"Category '{dto.Name}' already exists.");
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            IsActive = true
        };

        var saved = await _categoryRepository.AddAsync(category);
        return new CategoryDto
        {
            CategoryId = saved.CategoryId,
            Name = saved.Name,
            IsActive = saved.IsActive
        };
    }

    public async Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found.");
        }

        var existing = await _categoryRepository.GetByNameAsync(dto.Name);
        if (existing != null && existing.CategoryId != categoryId)
        {
            throw new ConflictException($"Category '{dto.Name}' already exists.");
        }

        category.Name = dto.Name.Trim();
        category.IsActive = dto.IsActive;

        await _categoryRepository.UpdateAsync(category);

        return new CategoryDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            IsActive = category.IsActive
        };
    }

    public async Task DeleteAsync(int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
        {
            throw new NotFoundException("Category not found.");
        }

        var inUse = await _categoryRepository.IsCategoryInUseAsync(categoryId);
        if (inUse)
        {
            // PRD Section 3.7: A category in use cannot be hard-deleted (deactivate instead)
            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);
        }
        else
        {
            category.IsActive = false;
            await _categoryRepository.UpdateAsync(category);
        }
    }
}
