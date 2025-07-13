using PetProfiles.Api.Models;

namespace PetProfiles.Api.Tests
{
	public class PetProfileTests
	{
		[Fact]
		public void PetProfile_ShouldHaveRequiredProperties()
		{
			// Arrange
			var petProfile = new PetProfile
			{
				Id = 1,
				Name = "Buddy",
				Breed = "Golden Retriever",
				Age = 3,
				ImageUrl = "https://example.com/buddy.jpg"
			};

			// Assert
			Assert.Equal(1, petProfile.Id);
			Assert.Equal("Buddy", petProfile.Name);
			Assert.Equal("Golden Retriever", petProfile.Breed);
			Assert.Equal(3, petProfile.Age);
			Assert.Equal("https://example.com/buddy.jpg", petProfile.ImageUrl);
		}
	}
}