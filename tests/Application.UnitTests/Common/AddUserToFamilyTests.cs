using Application.Dto;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Domain.Entities;
using Infrastructure.Interfaces;
using Midas.Services;
using Moq;

namespace Application.UnitTests.Common;

[TestFixture]
public class AddUserToFamilyTests
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

    public AddUserToFamilyTests()
    {
        var familyRepository = new Mock<IFamilyRepository>();
        var userClient = new Mock<IUserClient>();
        var mapper = AutoMapperConfig.Initialize();

        userClient.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((string email) => new UserDto
        {
            Email = email,
            Id = 1
        });
        familyRepository.Setup(x => x.GetFamilyById(It.IsAny<ulong>()))
            .ReturnsAsync((ulong id) => _families.FirstOrDefault(x => x.Id == id));
        familyRepository.Setup(x => x.AddUserToFamily(It.IsAny<ulong>(), It.IsAny<ulong>()))
            .ReturnsAsync((ulong userId, ulong familyId) => true);

        _familyService = new FamilyService(familyRepository.Object, mapper, userClient.Object, null);
    }

    [Test]
    public async Task ShouldReturnTrue_ForExistingEntity()
    {
        var dto = new AddUserToFamilyDto
        {
            Email = "test@test.com",
            FamilyId = 1
        };

        var result = await _familyService.AddUserToFamily(dto).ConfigureAwait(false);
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    [TestCase("test@test.com", 2ul)]
    [TestCase("tes2@test.com", 2137ul)]
    [TestCase("test3@test.com", 3ul)]
    public async Task ShouldReturnFalse_ForNotExistingEntity(string email, ulong id)
    {
        var dto = new AddUserToFamilyDto
        {
            Email = email,
            FamilyId = id
        };

        var result = await _familyService.AddUserToFamily(dto).ConfigureAwait(false);
        
        Assert.That(result, Is.False);
    }
}