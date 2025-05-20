using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace Carvisto.Tests.Services.Account
{
    [TestFixture]
    public class AccountServiceTests
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private CarvistoDbContext _context;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private AccountService _accountService;
        private ApplicationUser _testUser;

        [SetUp]
        public void Setip()
        {
            // Settings test user
            _testUser = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@example.com",
                ContactName = "Test User",
                ContactPhone = "1234567890",
            };
            
            // Settings mocks
            SetupMockUserManager();
            SetupMockSignInManager();
            SetupMockHttpContextAccessor();
            SetupMockDbContext();
            
            // Creating the AccountService instance
            _accountService = new AccountService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _context,
                _mockHttpContextAccessor.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private void SetupMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                null, null, null, null, null, null, null, null);
            
            // Setting methods UserManager
            _mockUserManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string email) => email == _testUser.Email ? _testUser : null);

            _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => id == _testUser.Id ? _testUser : null);
            
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(_testUser);

            _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(m => m.ChangePasswordAsync(
                It.IsAny<ApplicationUser>(),
                It.Is<string>(s => s == "correctPassword"),
                It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            
            _mockUserManager.Setup(m => m.ChangePasswordAsync(
                    It.IsAny<ApplicationUser>(), 
                    It.Is<string>(s => s != "correctPassword"), 
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password" }));

            _mockUserManager.Setup(m => m.CheckPasswordAsync(
                    It.IsAny<ApplicationUser>(), 
                    It.Is<string>(s => s == "correctPassword")))
                .ReturnsAsync(true);

            _mockUserManager.Setup(m => m.CheckPasswordAsync(
                    It.IsAny<ApplicationUser>(), 
                    It.Is<string>(s => s != "correctPassword")))
                .ReturnsAsync(false);
        }

        private void SetupMockSignInManager()
        {
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object,
                null, null, null, null);
            
            _mockSignInManager.Setup(m => m.PasswordSignInAsync(
                It.Is<string>(s => s == _testUser.Email),
                It.Is<string>(s => s == "correctPassword"),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
            
            _mockSignInManager.Setup(m => m.PasswordSignInAsync(
                    It.IsAny<string>(),
                    It.Is<string>(s => s != "correctPassword"),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Failed);
            
            _mockSignInManager.Setup(m => m.SignInAsync(
                    It.IsAny<ApplicationUser>(),
                    It.IsAny<bool>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            
            _mockSignInManager.Setup(m => m.SignOutAsync())
                .Returns(Task.CompletedTask);
        }

        private void SetupMockHttpContextAccessor()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,_testUser.Id)
            };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext { User = claimsPrincipal };
            
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(x => x.HttpContext)
                .Returns(context);
        }

        private void SetupMockDbContext()
        {
            var options = new DbContextOptionsBuilder<CarvistoDbContext>()
                .UseInMemoryDatabase(databaseName: $"AccountServiceTest_{Guid.NewGuid()}")
                .Options;
            
            _context = new CarvistoDbContext(options);
            
            // Add test trip
            _context.Trips.Add(new Models.Trip
            {
                Id = 1,
                DriverId = _testUser.Id,
                StartLocation = "Bialystok",
                EndLocation = "Warszawa",
                DepartureDateTime = DateTime.Now.AddDays(1),
                VehicleBrand = "Toyota",
                Comments = "Test comment",
                Seats = 3,
                Price = 50,
            });
            
            _context.SaveChanges();
        }

        [Test]
        [Category("Account.Register")]
        public async Task RegisterUserAsync_ShouldCreateUser_AndReturnSuccess()
        {
            // Arrange
            string email = "new@example.com";
            string password = "Password123!";
            string contactName = "New User";
            string contactPhone = "9876543210";
            
            // Act
            var result = await _accountService.RegisterUserAsync(email, password, contactName, contactPhone);
            
            // Assert
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.CreateAsync(
                It.Is<ApplicationUser>( u =>
                    u.Email == email &&
                    u.ContactName == contactName &&
                    u.ContactPhone == contactPhone),
                password),
                Times.Once);
        }

        [Test]
        [Category("Account.Login")]
        public async Task LoginUserAsync_WithCorrectCredentials_ShouldReturnSuccess()
        {
            // Arrange
            string email = _testUser.Email;
            string password = "correctPassword";
            bool rememberMe = true;
            
            // Act
            var result = await _accountService.LoginUserAsync(email, password, rememberMe);
            
            // Assert
            Assert.That(result.Succeeded, Is.True);
            _mockSignInManager.Verify(m => m.PasswordSignInAsync(
                email, password, rememberMe, false),
                Times.Once);
        }

        [Test]
        [Category("Account.Login")]
        public async Task LoginUserAsync_WithIncorrectCredentials_ShouldReturnFailure()
        {
            // Arrange
            string email = _testUser.Email;
            string password = "wrongPassword";
            bool rememberMe = false;
            
            // Act
            var result = await _accountService.LoginUserAsync(email, password, rememberMe);
            
            // Assert
            Assert.That(result.Succeeded, Is.False);
        }

        [Test]
        [Category("Account.SignIn")]
        public async Task SignInUserAsync_ShouldSignInUser()
        {
            // Arrange
            bool isPersistent = true;
            
            // Act
            await _accountService.SignInUserAsync(_testUser, isPersistent);
            
            // Assert
            _mockSignInManager.Verify(m => m.SignInAsync(
                    _testUser, isPersistent, null),
                Times.Once);
        }
        
        [Test]
        [Category("Account.Logout")]
        public async Task LogoutAsync_ShouldSignOutUser()
        {
            // Act
            await _accountService.LogoutAsync();
            
            // Assert
            _mockSignInManager.Verify(m => m.SignOutAsync(), Times.Once);
        }

        [Test]
        [Category("Account.User")]
        public async Task GetUserByEmailAsync_WithExistingEmail_ShouldReturnUser()
        {
            // Arrange
            string email = _testUser.Email;
            
            // Act
            var result = await _accountService.GetUserByEmailAsync(email);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Email, Is.EqualTo(email));
        }

        [Test]
        [Category("Account.User")]
        public async Task GetUserByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
        {
            // Arrange
            string email = "nonexistent@example.com";

            // Act
            var result = await _accountService.GetUserByEmailAsync(email);

            // Assert
            Assert.That(result, Is.Null);
        }
        
        [Test]
        [Category("Account.User")]
        public async Task GetCurrentUserAsync_ShouldReturnCurrentUser()
        {
            // Act
            var result = await _accountService.GetCurrentUserAsync();
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(_testUser.Id));
        }

        [Test]
        [Category("Account.Update")]
        public async Task UpdateUserAsync_ShouldUpdateUserInfo()
        {
            // Arrange
            _testUser.ContactName = "Updated Name";
            _testUser.ContactPhone = "0987654321";
            
            // Act
            var result = await _accountService.UpdateUserAsync(_testUser);
            
            // Assert
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.UpdateAsync(_testUser), Times.Once);
        }

        [Test]
        [Category("Account.Role")]
        public async Task AddUserToRoleAsync_ShouldAddUserToSpecifiedRole()
        {
            // Arrange
            string role = "Driver";
            
            // Act
            await _accountService.AddUserToRoleAsync(_testUser, role);
            
            // Assert
            _mockUserManager.Verify(m => m.AddToRoleAsync(_testUser, role), Times.Once);
        }

        [Test]
        [Category("Account.ViewModel")]
        public async Task GetAccountViewModelAsync_WithValidUserId_ShouldReturnViewModel()
        {
            // Arrange
            string userId = _testUser.Id;
            
            // Act
            var result = await _accountService.GetAccountViewModelAsync(userId);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.User, Is.Not.Null);
            Assert.That(result.User.Id, Is.EqualTo(userId));
            Assert.That(result.UserTrips, Is.Not.Null);
            Assert.That(result.UserTrips.Count, Is.EqualTo(1));
            Assert.That(result.ChangePassword, Is.Not.Null);
        }

        [Test]
        [Category("Account.ViewModel")]
        public async Task GetAccountViewModelAsync_WithInvalidUserId_ShouldReturnNull()
        {
            // Arrange
            string userId = "invalid-user-id";
            
            // Act
            var result = await _accountService.GetAccountViewModelAsync(userId);
            
            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        [Category("Account.Password")]
        public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ShouldReturnSuccess()
        {
            // Arrange
            string currentPassword = "correctPassword";
            string newPassword = "NewPassword123!";
            
            // Act
            var result = await _accountService.ChangePasswordAsync(_testUser, currentPassword, newPassword);
            
            // Assert
            Assert.That(result.Succeeded, Is.True);
            _mockUserManager.Verify(m => m.ChangePasswordAsync(
                _testUser, currentPassword, newPassword),
                Times.Once);
        }

        [Test]
        [Category("Account.Password")]
        public async Task ChangePasswordAsync_WithIncorrectCurrentPassword_ShouldReturnFailure()
        {
            // Arrange
            string currentPassword = "wrongPassword";
            string newPassword = "NewPassword123!";
            
            // Act
            var result = await _accountService.ChangePasswordAsync(_testUser, currentPassword, newPassword);
            
            // Assert
            Assert.That(result.Succeeded, Is.False);
        }
        
        [Test]
        [Category("Account.Password")]
        public async Task CheckPasswordAsync_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            string password = "correctPassword";
            
            // Act
            var result = await _accountService.CheckPasswordAsync(_testUser, password);
            
            // Assert
            Assert.That(result, Is.True);
        }
        
        [Test]
        [Category("Account.Password")]
        public async Task CheckPasswordAsync_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            string password = "wrongPassword";
            
            // Act
            var result = await _accountService.CheckPasswordAsync(_testUser, password);
            
            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        [Category("Account.ProfileImage")]
        public async Task SaveProfileImageAsync_ShouldSaveImageAndReturnPath()
        {
            // Arrange
            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            try
            {
                string fileName = "testImage.jpg";
                byte[] imageBytes = new byte[] { 1, 2, 3, 4, 5 };
                
                // User without profile image
                var user = new ApplicationUser
                {
                    Id = "user-without-image",
                    Email = "noimage@example.com",
                    ContactName = "No Image User",
                    ContactPhone = "1234567890",
                };
                
                // Act
                var result = await _accountService.SaveProfileImageAsync(user, tempDir, fileName, imageBytes);
                
                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Does.StartWith("/images/profiles/"));
                Assert.That(result, Does.Contain(user.Id));
                Assert.That(result, Does.EndWith(".jpg"));
                
                // Check if the file was created
                var savedFiles = Directory.GetFiles(tempDir);
                Assert.That(savedFiles.Length, Is.EqualTo(1));
            }
            finally
            {
                // Clean up
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        [Test]
        [Category("Account.ProfileImage")]
        public async Task DeleteProfileImageAsync_WithoutImage_ShouldReturnFalse()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user-without-image",
                Email = "noimage@example.com",
                ContactName = "No Image User",
                ContactPhone = "1234567890",
                ProfileImagePath = null
            };
            
            // Act
            var result = await _accountService.DeleteProfileImageAsync(user);
            
            // Assert
            Assert.That(result, Is.False);
        }
    }
}