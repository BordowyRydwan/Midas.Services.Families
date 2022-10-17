using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using MockQueryable.Moq;
using Moq;

namespace Infrastructure.UnitTests.Repositories;

[TestFixture]
public class AddNewFamilyTests
{
    private readonly IFamilyRepository _repository;

    private readonly IList<FamilyRole> _mockFamilyRoles = new List<FamilyRole>
    {
        new()
        {
            Id = 1,
            Name = "Main administrator"
        }
    };

    private IList<Family> _data = new List<Family>
    {
        new()
        {
            Id = 1,
            Name = "Test 1"
        }
    };

    public AddNewFamilyTests()
    {
        var mockContext = new Mock<FamilyDbContext>();
        var mockData = _data.AsQueryable().BuildMockDbSet();
        var mockFamilyRoles = _mockFamilyRoles.AsQueryable().BuildMockDbSet();
        
        mockContext.Setup(x => x.Families).Returns(mockData.Object);
        mockContext.Setup(x => x.FamilyRoles).Returns(mockFamilyRoles.Object);
        mockContext.Setup(m => m.AddAsync(It.IsAny<Family>(), default))
            .Callback<Family, CancellationToken>((family, _) =>
            {
                family.Id = (ulong)(_data.Count + 1);
                _data.Add(family);
            });

        _repository = new FamilyRepository(mockContext.Object);
    }
    
    [TearDown]
    public void ClearList()
    {
        _data = new List<Family>
        {
            new()
            {
                Id = 1,
                Name = "Test 1"
            }
        };
    }

    [Test]
    public async Task AddFamilyWithEmptyName_ShouldThrowFamilyException()
    {
        var testInstance = new Family { Name = "", FounderId = 1 };
        var result = Assert.ThrowsAsync<FamilyException>(() => _repository.AddNewFamily(testInstance));
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<FamilyException>());
            Assert.That(_data, Has.Count.EqualTo(1));
        });
    }
    
    [Test]
    public async Task AddFamilyWithExistingName_ShouldThrowFamilyException()
    {
        var testInstance = new Family { Name = "Test 1", FounderId = 1 };
        var result = Assert.ThrowsAsync<FamilyException>(() => _repository.AddNewFamily(testInstance));
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<FamilyException>());
            Assert.That(_data, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task AddFamilyWithProperModel_ShouldGoOK()
    {
        var testInstance = new Family { Name = "Test 2", FounderId = 1 };
        
        await _repository.AddNewFamily(testInstance).ConfigureAwait(false);
        Assert.That(_data, Has.Count.EqualTo(2));
    }
}