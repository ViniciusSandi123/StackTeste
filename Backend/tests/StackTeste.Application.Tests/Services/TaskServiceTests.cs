using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackTeste.Application.DTOs.Task;
using StackTeste.Application.Mappings;
using StackTeste.Application.Services;
using StackTeste.Application.Validations.Task;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;
using Xunit;
using TaskStatus = StackTeste.Domain.Helpers.Enums.TaskStatus;

namespace StackTeste.Application.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly Mock<ILeadRepository> _leadRepoMock;
        private readonly IMapper _mapper;
        private readonly IValidator<TaskCreateDto> _createValidator;
        private readonly IValidator<TaskUpdateDto> _updateValidator;
        private readonly TaskService _sut;

        public TaskServiceTests()
        {
            _taskRepoMock = new Mock<ITaskRepository>();
            _leadRepoMock = new Mock<ILeadRepository>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping>(), NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();

            _createValidator = new TaskCreateDtoValidator();
            _updateValidator = new TaskUpdateDtoValidator();

            _sut = new TaskService(
                _taskRepoMock.Object,
                _leadRepoMock.Object,
                _mapper,
                _createValidator,
                _updateValidator);
        }

        [Fact]
        public async Task GetByLeadAsync_QuandoLeadExiste_DeveRetornarTasks()
        {
            _leadRepoMock
                .Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var tasks = new List<TaskItem>
            {
                new() { Id = 10, LeadId = 1, Title = "Tarefa 1", Status = TaskStatus.Todo },
                new() { Id = 11, LeadId = 1, Title = "Tarefa 2", Status = TaskStatus.Doing }
            };
            _taskRepoMock
                .Setup(r => r.GetByLeadAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            var result = await _sut.GetByLeadAsync(1);

            result.Success.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value!.Should().HaveCount(2);
            result.Value!.First().Title.Should().Be("Tarefa 1");
        }

        [Fact]
        public async Task GetByLeadAsync_QuandoLeadNaoExiste_DeveRetornarFail()
        {
            _leadRepoMock
                .Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.GetByLeadAsync(99);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("99");
            _taskRepoMock.Verify(r => r.GetByLeadAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_QuandoLeadExisteEDtoValido_DeveCriarTask()
        {
            _leadRepoMock
                .Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            TaskItem? captured = null;
            _taskRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .Callback((TaskItem t, CancellationToken _) =>
                {
                    t.Id = 100;
                    captured = t;
                })
                .ReturnsAsync((TaskItem t, CancellationToken _) => t);

            var dto = new TaskCreateDto("Ligar pro cliente", DueDate: null, Status: TaskStatus.Todo);
            var result = await _sut.CreateAsync(1, dto);

            result.Success.Should().BeTrue();
            result.Value!.Title.Should().Be("Ligar pro cliente");
            result.Value.LeadId.Should().Be(1);

            captured.Should().NotBeNull();
            captured!.LeadId.Should().Be(1);
            _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_QuandoStatusNaoInformado_DeveAssumirTodo()
        {
            _leadRepoMock
                .Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            TaskItem? captured = null;
            _taskRepoMock
                .Setup(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
                .Callback((TaskItem t, CancellationToken _) => captured = t)
                .ReturnsAsync((TaskItem t, CancellationToken _) => t);

            var dto = new TaskCreateDto("Sem status", DueDate: null, Status: null);
            var result = await _sut.CreateAsync(1, dto);

            result.Success.Should().BeTrue();
            captured.Should().NotBeNull();
            captured!.Status.Should().Be(TaskStatus.Todo);
        }

        [Fact]
        public async Task CreateAsync_QuandoLeadNaoExiste_DeveRetornarFail()
        {
            _leadRepoMock
                .Setup(r => r.ExistsAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dto = new TaskCreateDto("Tarefa", null, TaskStatus.Todo);
            var result = await _sut.CreateAsync(99, dto);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("99");
            _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_ComDtoInvalido_DeveLancarValidationException()
        {
            var dto = new TaskCreateDto("", null, TaskStatus.Todo);
            Func<Task> act = async () => await _sut.CreateAsync(1, dto);

            await act.Should().ThrowAsync<ValidationException>();
            _leadRepoMock.Verify(r => r.ExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _taskRepoMock.Verify(r => r.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_QuandoTaskExiste_DeveAtualizar()
        {
            var existing = new TaskItem
            {
                Id = 10,
                LeadId = 1,
                Title = "Antiga",
                Status = TaskStatus.Todo
            };
            _taskRepoMock
                .Setup(r => r.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var dto = new TaskUpdateDto("Nova", DueDate: null, Status: TaskStatus.Done);
            var result = await _sut.UpdateAsync(1, 10, dto);

            result.Success.Should().BeTrue();
            result.Value!.Title.Should().Be("Nova");
            result.Value.Status.Should().Be(TaskStatus.Done);

            _taskRepoMock.Verify(
                r => r.UpdateAsync(It.Is<TaskItem>(t => t.Title == "Nova" && t.Status == TaskStatus.Done),
                                   It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuandoTaskNaoPertenceAoLead_DeveRetornarFail()
        {
            _taskRepoMock
                .Setup(r => r.GetByIdAsync(1, 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem?)null);

            var dto = new TaskUpdateDto("X", null, TaskStatus.Todo);
            var result = await _sut.UpdateAsync(1, 999, dto);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("999");
            _taskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ComDtoInvalido_DeveLancarValidationException()
        {
            var dto = new TaskUpdateDto("", null, TaskStatus.Todo);
            Func<Task> act = async () => await _sut.UpdateAsync(1, 10, dto);

            await act.Should().ThrowAsync<ValidationException>();
            _taskRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _taskRepoMock.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_QuandoTaskExiste_DeveDeletar()
        {
            var existing = new TaskItem { Id = 10, LeadId = 1, Title = "X" };
            _taskRepoMock
                .Setup(r => r.GetByIdAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var result = await _sut.DeleteAsync(1, 10);
            result.Success.Should().BeTrue();
            _taskRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoTaskNaoExiste_DeveRetornarFail()
        {
            _taskRepoMock
                .Setup(r => r.GetByIdAsync(1, 999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TaskItem?)null);

            var result = await _sut.DeleteAsync(1, 999);
            result.Success.Should().BeFalse();
            result.Error.Should().Contain("999");
            _taskRepoMock.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
