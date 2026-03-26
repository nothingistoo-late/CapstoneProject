namespace CapstoneProject.Domain.Common;

public static class EntityExtension
{
    public static void InitializeEntity(this IEntityLike entity, Guid? userId = null)
    {
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        entity.CreatedBy = userId ?? Guid.Empty; //Guid.Empty is for system
    }

    public static void UpdateEntity(this IEntityLike entity, Guid? userId = null, IEntityLike? oldEntity = null)
    {
        if (oldEntity != null)
        {
            entity.CreatedAt = oldEntity.CreatedAt;
            entity.CreatedBy = oldEntity.CreatedBy;
        }
        
        entity.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }

    public static void SoftDeleteEntity(this IEntityLike entity, Guid? userId = null)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.DeletedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
            baseEntity.DeletedBy = userId ?? Guid.Empty;
        }
    }

    public static void RestoreEntity(this IEntityLike entity, Guid? userId = null)
    {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = false;
            baseEntity.DeletedAt = null;
            baseEntity.DeletedBy = null;
        }
        entity.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }

    /// <summary>
    /// Deactivate user account - safer than deletion for AppUser
    /// </summary>
    public static void DeactivateUser(this IEntityLike entity, Guid? userId = null)
    {
        entity.Status = Enums.EntityStatusEnum.Inactive;
        entity.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }

    /// <summary>
    /// Reactivate user account
    /// </summary>
    public static void ReactivateUser(this IEntityLike entity, Guid? userId = null)
    {
        entity.Status = Enums.EntityStatusEnum.Active;
        entity.UpdatedAt = CapstoneProject.Domain.Common.VietnamDateTime.Now;
        entity.UpdatedBy = userId ?? Guid.Empty;
    }
}
