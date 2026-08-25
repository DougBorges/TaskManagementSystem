using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskItemDto>> GetUserTasksAsync(int userId, CancellationToken cancellationToken = default);
    Task<TaskItemDto> GetTaskByIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<TaskItemDto> CreateTaskAsync(int userId, TaskItemCreateDto dto, CancellationToken cancellationToken = default);
    Task<TaskItemDto> UpdateTaskAsync(int taskId, int userId, TaskItemUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(int taskId, int userId, CancellationToken cancellationToken = default);
}
