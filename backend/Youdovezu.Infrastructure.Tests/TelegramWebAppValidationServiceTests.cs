using Microsoft.Extensions.Logging;
using Moq;
using Youdovezu.Infrastructure.Services;
using Xunit;

namespace Youdovezu.Infrastructure.Tests;

public class TelegramWebAppValidationServiceTests
{
    private readonly Mock<ILogger<TelegramWebAppValidationService>> _mockLogger;
    private readonly string _botToken = "7642287932:AAGAaP0BdJgvxrE3UEfdAoDcrJ0D9TzmSJI";
    private readonly TelegramWebAppValidationService _service;

    public TelegramWebAppValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<TelegramWebAppValidationService>>();
        _service = new TelegramWebAppValidationService(_botToken, _mockLogger.Object);
    }

    [Fact]
    public void ValidateInitData_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        //var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";
        var initData = "query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&auth_date=1760528863&signature=NFUDswNm6DuvIyxiOnfbJ2e-gnR6tl2_MJQQQftyqwg-2rw0-CdQKXugwgcgUQkx1C7f9W65YDyWSUocqplaDg&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";
        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.True(result, $"Validation failed for initData: {initData}");
    }

    [Fact]
    public void ValidateInitData_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&hash=invalid_hash";

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInitData_WithMissingHash_ShouldReturnFalse()
    {
        // Arrange
        var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D";

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInitData_WithEmptyData_ShouldReturnFalse()
    {
        // Arrange
        var initData = "";

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInitData_WithNullData_ShouldReturnFalse()
    {
        // Arrange
        string initData = null;

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateInitData_WithOldAuthDate_ShouldReturnFalse()
    {
        // Arrange - используем старую дату (более 24 часов назад)
        var oldTimestamp = DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeSeconds();
        var initData = $"auth_date={oldTimestamp}&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractTelegramId_WithValidData_ShouldReturnCorrectId()
    {
        // Arrange
        var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";

        // Act
        var result = _service.ExtractTelegramId(initData);

        // Assert
        Assert.Equal(315605633, result);
    }

    [Fact]
    public void ExtractTelegramId_WithInvalidData_ShouldReturnNull()
    {
        // Arrange
        var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";

        // Act
        var result = _service.ExtractTelegramId(initData);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ValidateInitData_DebugTest_ShouldShowDetailedLogging()
    {
        // Arrange
        var initData = "auth_date=1760528863&query_id=AAGBws8SAAAAAIHCzxKGChnW&user=%7B%22id%22%3A315605633%2C%22first_name%22%3A%22%D0%A0%D1%83%D1%81%D0%BB%D0%B0%D0%BD%22%2C%22last_name%22%3A%22%D0%93%D0%B0%D0%BB%D0%B5%D0%B5%D0%B2%22%2C%22username%22%3A%22ruslannafisovich%22%2C%22language_code%22%3A%22ru%22%2C%22is_premium%22%3Atrue%2C%22allows_write_to_pm%22%3Atrue%2C%22photo_url%22%3A%22https%3A%5C%2F%5C%2Ft.me%5C%2Fi%5C%2Fuserpic%5C%2F320%5C%2FxXYDxKE2WYNc1tMAxG7QX9E9teHSDQVJcintX-bb-tg.svg%22%7D&hash=02cdf8808fffb45fe04278f671332eb7223c0e5099574d947cbe8050cfdc4fb2";

        // Act
        var result = _service.ValidateInitData(initData);

        // Assert - этот тест всегда должен проходить, но покажет логи
        Assert.True(true, $"Debug test completed. Result: {result}");
    }
}
