using Youdovezu.Application.Interfaces;
using Youdovezu.Domain.Entities;
using Youdovezu.Domain.Interfaces;

namespace Youdovezu.Application.Services;

/// <summary>
/// Сервис для работы с поездками
/// </summary>
public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Конструктор сервиса
    /// </summary>
    /// <param name="tripRepository">Репозиторий поездок</param>
    /// <param name="userRepository">Репозиторий пользователей</param>
    public TripService(ITripRepository tripRepository, IUserRepository userRepository)
    {
        _tripRepository = tripRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Создать новую поездку
    /// </summary>
    public async Task<Trip> CreateTripAsync(
        long userId,
        string fromAddress,
        string fromSettlement,
        double? fromLatitude,
        double? fromLongitude,
        string toAddress,
        string toSettlement,
        double? toLatitude,
        double? toLongitude,
        string? comment)
    {
        // Проверяем существование пользователя
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("Пользователь не найден", nameof(userId));
        }

        // Создаем новую поездку (любой пользователь может создавать поездки)
        var trip = new Trip
        {
            UserId = userId,
            FromAddress = fromAddress,
            FromSettlement = fromSettlement,
            FromLatitude = fromLatitude,
            FromLongitude = fromLongitude,
            ToAddress = toAddress,
            ToSettlement = toSettlement,
            ToLatitude = toLatitude,
            ToLongitude = toLongitude,
            Comment = comment,
            Status = TripStatus.Active
        };

        return await _tripRepository.CreateAsync(trip);
    }

    /// <summary>
    /// Получить все поездки пользователя
    /// </summary>
    public async Task<List<Trip>> GetUserTripsAsync(long userId)
    {
        return await _tripRepository.GetByUserIdAsync(userId);
    }

    /// <summary>
    /// Получить поездку по ID
    /// </summary>
    public async Task<Trip?> GetTripByIdAsync(long tripId)
    {
        return await _tripRepository.GetByIdAsync(tripId);
    }

    /// <summary>
    /// Обновить поездку
    /// </summary>
    public async Task<Trip> UpdateTripAsync(
        long tripId,
        long userId,
        string fromAddress,
        string fromSettlement,
        double? fromLatitude,
        double? fromLongitude,
        string toAddress,
        string toSettlement,
        double? toLatitude,
        double? toLongitude,
        string? comment)
    {
        // Получаем поездку
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException("Поездка не найдена", nameof(tripId));
        }

        // Проверяем, что пользователь является владельцем поездки
        if (trip.UserId != userId)
        {
            throw new UnauthorizedAccessException("Вы не являетесь владельцем этой поездки");
        }

        // Обновляем данные поездки
        trip.FromAddress = fromAddress;
        trip.FromSettlement = fromSettlement;
        trip.FromLatitude = fromLatitude;
        trip.FromLongitude = fromLongitude;
        trip.ToAddress = toAddress;
        trip.ToSettlement = toSettlement;
        trip.ToLatitude = toLatitude;
        trip.ToLongitude = toLongitude;
        trip.Comment = comment;

        return await _tripRepository.UpdateAsync(trip);
    }

    /// <summary>
    /// Удалить поездку
    /// </summary>
    public async Task<Trip> DeleteTripAsync(long tripId, long userId)
    {
        // Получаем поездку
        var trip = await _tripRepository.GetByIdAsync(tripId);
        if (trip == null)
        {
            throw new ArgumentException("Поездка не найдена", nameof(tripId));
        }

        // Проверяем, что пользователь является владельцем поездки
        if (trip.UserId != userId)
        {
            throw new UnauthorizedAccessException("Вы не являетесь владельцем этой поездки");
        }

        // Удаляем поездку (мягкое удаление)
        return await _tripRepository.DeleteAsync(trip);
    }
}

