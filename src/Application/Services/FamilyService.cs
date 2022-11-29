using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Dto;
using Application.Interfaces;
using AutoMapper;
using Domain.Consts;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Interfaces;
using Midas.Services;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMapper _mapper;
    private readonly IUserClient _userClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FamilyService(
        IFamilyRepository familyRepository, IMapper mapper, IUserClient userClient, IHttpContextAccessor contextAccessor)
    {
        _familyRepository = familyRepository;
        _mapper = mapper;
        _userClient = userClient;
        _httpContextAccessor = contextAccessor;
    }
    
    public async Task<AddNewFamilyReturnDto> AddNewFamily(AddNewFamilyDto dto)
    {
        var familyEntity = _mapper.Map<AddNewFamilyDto, Family>(dto);
        var userEntity = await _userClient.GetUserByIdAsync((long)dto.FounderId).ConfigureAwait(false);

        if (userEntity is null)
        {
            throw new UserException("");
        }

        var familyId = await _familyRepository.AddNewFamily(familyEntity);

        return new AddNewFamilyReturnDto
        {
            Id = familyId,
            Name = dto.Name
        };
    }

    public async Task<bool> DeleteFamily(ulong id)
    {
        if (await CheckIfUserInFamilyHasRole(id, FamilyRoles.MainAdministrator))
        {
            return await _familyRepository.DeleteFamily(id).ConfigureAwait(false);
        }

        return false;
    }

    public async Task<bool> AddUserToFamily(AddUserToFamilyDto dto)
    {
        var userEntity = await _userClient.GetUserByEmailAsync(dto.Email).ConfigureAwait(false);
        var familyEntity = await _familyRepository.GetFamilyById(dto.FamilyId).ConfigureAwait(false);

        if (userEntity is null || familyEntity is null)
        {
            return false;
        }

        return await _familyRepository.AddUserToFamily((ulong)userEntity.Id, familyEntity.Id).ConfigureAwait(false);
    }

    public async Task<bool> DeleteUserFromFamily(DeleteUserFromFamilyDto dto)
    {
        if (!await CheckIfUserInFamilyHasRole(dto.FamilyId, FamilyRoles.MainAdministrator))
        {
            return false;
        }
        
        var userEntity = await _userClient.GetUserByEmailAsync(dto.Email).ConfigureAwait(false);
        var familyEntity = await _familyRepository.GetFamilyById(dto.FamilyId).ConfigureAwait(false);

        if (userEntity is null || familyEntity is null)
        {
            return false;
        }

        return await _familyRepository.DeleteUserFromFamily((ulong)userEntity.Id, familyEntity.Id).ConfigureAwait(false);
    }

    public async Task<bool> SetUserFamilyRole(SetUserFamilyRoleDto dto)
    {
        if (!await CheckIfUserInFamilyHasRole(dto.FamilyId, FamilyRoles.MainAdministrator))
        {
            return false;
        }
        
        var userEntity = await _userClient.GetUserByEmailAsync(dto.Email).ConfigureAwait(false);
        
        if (userEntity is null)
        {
            return false;
        }
        
        var userFamilyRole = _mapper.Map<SetUserFamilyRoleDto, UserFamilyRole>(dto, opt => 
            opt.AfterMap((_, dest) => dest.UserId = (ulong)userEntity.Id));
        
        return await _familyRepository.SetUserFamilyRole(userFamilyRole).ConfigureAwait(false);
    }

    public async Task<UserFamilyRoleListDto> GetFamilyMembershipsForActiveUser()
    {
        var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
        var handler = new JwtSecurityTokenHandler();

        var userId = handler.ReadJwtToken(token).Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var userFamilyRoles = await _familyRepository.GetFamilyMemberRolesForUser(Convert.ToUInt64(userId))
            .ConfigureAwait(false);
        var returnList = new List<UserFamilyRoleDto>();

        foreach (var item in userFamilyRoles)
        {
            var dto = await MapUserFamilyRoleToDto(item).ConfigureAwait(false);
            returnList.Add(dto);
        }
        
        return _mapper.Map<List<UserFamilyRoleDto>, UserFamilyRoleListDto>(returnList.ToList());
    }
    
    public async Task<UserFamilyRoleListDto> GetFamilyMembers(ulong familyId)
    {
        var entity = await _familyRepository.GetFamilyMembers(familyId).ConfigureAwait(false);
        var returnList = new List<UserFamilyRoleDto>();

        foreach (var item in entity)
        {
            var dto = await MapUserFamilyRoleToDto(item).ConfigureAwait(false);
            returnList.Add(dto);
        }
        
        return _mapper.Map<List<UserFamilyRoleDto>, UserFamilyRoleListDto>(returnList.ToList());
    }

    private async Task<UserFamilyRoleDto> MapUserFamilyRoleToDto(UserFamilyRole userFamilyRole)
    {
        var familyRoleDto = _mapper.Map<FamilyRole, FamilyRoleDto>(userFamilyRole.FamilyRole);
        var familyDto = _mapper.Map<Family, FamilyDto>(userFamilyRole.Family);
        var userDto = await _userClient.GetUserByIdAsync(Convert.ToInt64(userFamilyRole.UserId)).ConfigureAwait(false);

        var userFamilyRoleDto = new UserFamilyRoleDto
        {
            User = userDto,
            FamilyRole = familyRoleDto,
            Family = familyDto
        };
        
        return userFamilyRoleDto;
    }

    private async Task<bool> CheckIfUserInFamilyHasRole(ulong familyId, FamilyRoles role)
    {
        var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
        var handler = new JwtSecurityTokenHandler();
        var userId = Convert.ToUInt64(handler.ReadJwtToken(token).Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var userFamilyRoles = await _familyRepository.GetFamilyMemberRolesForUser(userId).ConfigureAwait(false);
        var userInFamilyRole = userFamilyRoles?.SingleOrDefault(x => x.FamilyId == familyId);

        if (userInFamilyRole is null)
        {
            return false;
        }
        
        return (int)userInFamilyRole.FamilyRoleId == (int)role;
    }
}