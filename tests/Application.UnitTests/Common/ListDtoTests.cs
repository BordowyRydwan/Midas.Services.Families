using Application.Dto;
using Application.Interfaces;

namespace Application.UnitTests.Common;

[TestFixture]
public class ListDtoTests
{
    [Test]
    [TestCase(typeof(UserFamilyRoleListDto))]
    public void ShouldHaveCorrectListDtoProperties(Type dto)
    {
        Has.Property("Count").And.Property("Items");
    }

    [Test]
    [TestCase(typeof(UserFamilyRoleListDto), typeof(IListDto<UserFamilyRoleDto>))]
    public void ShouldDeriveFromIListDto(Type listDto, Type objectDto)
    {
        var result = objectDto.IsAssignableFrom(listDto);
        Assert.That(result, Is.True);
    }

    [Test]
    public void ShouldGettersAndSettersWorkCorrectly()
    {
        var obj = new UserFamilyRoleListDto();

        Assert.Multiple(() =>
        {
            Assert.That(0, Is.EqualTo(obj.Count));
            Assert.That(obj.Items, Is.Null);
        });
    }
}