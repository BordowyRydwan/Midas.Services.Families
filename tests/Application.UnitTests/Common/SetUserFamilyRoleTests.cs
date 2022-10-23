using Application.Dto;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Midas.Services;
using Moq;

namespace Application.UnitTests.Common;

[TestFixture]
public class SetUserFamilyRoleTests
{
    private readonly IFamilyService _familyService;

    private IList<Family> _families = new List<Family>
    {
        new()
        {
            Id = 1,
            Name = "Test 1"
        }
    };
    private IList<UserFamilyRole> _userFamilyRoles = new List<UserFamilyRole>
    {
        new()
        {
            UserId = 8,
            FamilyId = 1,
            FamilyRoleId = 1
        }
    };

    public SetUserFamilyRoleTests()
    {
        var userClient = new Mock<IUserClient>();
        var familyRepository = new Mock<IFamilyRepository>();
        var mapper = AutoMapperConfig.Initialize();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        
        //id = 8, Test Testowy, test@test.com
        context.Request.Headers["Authorization"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwOi8vYXV0aC5taWRhcy5jb20iLCJpYXQiOjE2NjY1NTE3MjQsImV4cCI6MTY2NjU1Mjc0NiwiYXVkIjoiaHR0cDovL2F1dGgubWlkYXMuY29tIiwic3ViIjoianJvY2tldEBleGFtcGxlLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiOCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJUZXN0IFRlc3Rvd3kiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbCI6InRlc3RAdGVzdC5jb20ifQ.5kwWMIfEKLHTBJLydZp-cy8sF-oCLb_dPlfUg4rHijI";
        mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(context);

        userClient.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => new UserDto
            {
                Email = email,
                Id = 1
            });
        
        familyRepository.Setup(x => x.GetFamilyMemberRolesForUser(It.IsAny<ulong>()))
            .ReturnsAsync((ulong id) => _userFamilyRoles);
        familyRepository.Setup(x => x.GetFamilyById(It.IsAny<ulong>()))
            .ReturnsAsync((ulong id) => _families.FirstOrDefault(x => x.Id == id));
        familyRepository.Setup(x => x.SetUserFamilyRole(It.IsAny<UserFamilyRole>()))
            .ReturnsAsync((UserFamilyRole entity) =>
            {
                return _families.Select(x => x.Id).Contains(entity.FamilyId);
            });

        _familyService = new FamilyService(familyRepository.Object, mapper, userClient.Object, mockHttpContextAccessor.Object);
    }

    [Test]
    public async Task ShouldReturnTrue_ForExistingEntity()
    {
        var dto = new SetUserFamilyRoleDto
        {
            Email = "test@test.com",
            FamilyId = 1,
            FamilyRoleId = 3
        };

        var result = await _familyService.SetUserFamilyRole(dto).ConfigureAwait(false);
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    [TestCase("test@test.com", 2ul)]
    [TestCase("tes2@test.com", 2137ul)]
    [TestCase("test3@test.com", 3ul)]
    public async Task ShouldReturnFalse_ForNotExistingEntity(string email, ulong id)
    {
        var dto = new SetUserFamilyRoleDto
        {
            Email = email,
            FamilyId = id,
            FamilyRoleId = 3
        };

        var result = await _familyService.SetUserFamilyRole(dto).ConfigureAwait(false);
        
        Assert.That(result, Is.False);
    }
}