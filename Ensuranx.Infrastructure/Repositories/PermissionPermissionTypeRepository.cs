using Ensuranx.Application.Interfaces;
using Ensuranx.Application.Requests.Profile;
using Ensuranx.Domain.Entities;
using Ensuranx.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Ensuranx.Infrastructure.Repositories;

public class PermissionPermissionTypeRepository : IPermissionPermissionTypeRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionPermissionTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PermissionPermissionType>> GetPermissionPermissionTypeListForUpdate(UpdatePermission updatePermission)
    {
        List<PermissionPermissionType> permissionPermissionTypeList = new List<PermissionPermissionType>();

        foreach (Domain.Enums.Enum.Permission permission in System.Enum.GetValues(typeof(Domain.Enums.Enum.Permission)))
        {
            PermissionPermissionType permissionPermissionType = new PermissionPermissionType();

            if (permission == Domain.Enums.Enum.Permission.ViewMyProfile)
            {
                foreach (var item in updatePermission.ViewMyProfile.PermissionTypeIdList)
                {
                    permissionPermissionType = await _context.PermissionPermissionType.AsNoTracking().Where(x => x.PermissionId ==
                                                 updatePermission.ViewMyProfile.PermissionId && x.PermissionTypeId ==
                                                 item).FirstOrDefaultAsync();

                    permissionPermissionTypeList.Add(permissionPermissionType);
                }
                
            }
            else if (permission == Domain.Enums.Enum.Permission.ViewMyPlayingStatus)
            {
                foreach (var item in updatePermission.ViewMyPlayingStatus.PermissionTypeIdList)
                {
                    permissionPermissionType = await _context.PermissionPermissionType.AsNoTracking().Where(x => x.PermissionId ==
                                                 updatePermission.ViewMyPlayingStatus.PermissionId && x.PermissionTypeId ==
                                                 item).FirstOrDefaultAsync();

                    permissionPermissionTypeList.Add(permissionPermissionType);
                }
                
            }
            else if (permission == Domain.Enums.Enum.Permission.ViewMyLastPlayed)
            {
                foreach (var item in updatePermission.ViewMyLastPlayed.PermissionTypeIdList)
                {
                    permissionPermissionType = await _context.PermissionPermissionType.AsNoTracking().Where(x => x.PermissionId ==
                                                 updatePermission.ViewMyLastPlayed.PermissionId && x.PermissionTypeId ==
                                                 item).FirstOrDefaultAsync();

                    permissionPermissionTypeList.Add(permissionPermissionType);
                }
            }
            else if (permission == Domain.Enums.Enum.Permission.ViewMyLastSeen)
            {
                foreach (var item in updatePermission.ViewMyLastSeen.PermissionTypeIdList)
                {
                    permissionPermissionType = await _context.PermissionPermissionType.AsNoTracking().Where(x => x.PermissionId ==
                                                 updatePermission.ViewMyLastSeen.PermissionId && x.PermissionTypeId ==
                                                 item).FirstOrDefaultAsync();

                    permissionPermissionTypeList.Add(permissionPermissionType);
                }
            }
        }

        return permissionPermissionTypeList;
    }

    public async Task<PermissionType> GetPermissionTypeByPermissionTypeName(string name)
    {
        return await _context.PermissionType.AsNoTracking().Where(x => x.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<List<PermissionPermissionType>> GetPermissionPermissionTypeListByPermissionIdListAndPermissionTypeId(List<long> permissionIdList, long permissionTypeId)
    {
        return await _context.PermissionPermissionType.AsNoTracking().Where(x => x.PermissionTypeId == permissionTypeId && permissionIdList.Contains(x.PermissionId)).ToListAsync();
    }
}
