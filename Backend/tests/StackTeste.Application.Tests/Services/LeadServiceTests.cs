using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackTeste.Application.DTOs.Lead;
using StackTeste.Application.Mappings;
using StackTeste.Application.Services;
using StackTeste.Application.Validations.Lead;
using StackTeste.Domain.Common;
using StackTeste.Domain.Helpers.Enums;
using StackTeste.Domain.Interfaces;
using StackTeste.Domain.Models;
using Xunit;

namespace StackTeste.Application.Tests.Services
{
    public class LeadServiceTests
    {
        private readonly Mock<ILeadRepository> _leadRepoMock;
        private readonly Mock<ITaskRepository> _taskRepoMock;
        private readonly IMapper _mapper;
        private readonly IValidator<LeadCreateDto> _createValidator;
        private readonly IValidator<LeadUpdateDto> _updateValidator;
        private readonly LeadService _sut;

        public LeadServiceTests()
        {
            _leadRepoMock = new Mock<ILeadRepository>();
            _taskRepoMock = new Mock<ITaskRepository>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping>(), NullLoggerFactory.Instance);
            _mapper = config.CreateMapper();

            _createValidator = new LeadCreateDtoValidator();
            _updateValidator = new LeadUpdateDtoValidator();

            _sut = new LeadService(_leadRepoMock.Object,_taskRepoMock.Object, _mapper, _createValidator, _updateValidator);
        }

        [Fact]
        public async Task GetAllAsync_DeveRetornarPagedResultMapeado()
        {
            var leads = new List<Lead>
            {
                new() { Id = 1, Name = "Alice", Email = "alice@x.com", Status = LeadStatus.New },
                new() { Id = 2, Name = "Bob",   Email = "bob@x.com",   Status = LeadStatus.Qualified }
            };
            var paged = new PagedResult<Lead>(leads, totalCount: 2, page: 1, pageSize: 10);

            _leadRepoMock
                .Setup(r => r.GetAllAsync(null, null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            var result = await _sut.GetAllAsync(null, null, 1, 10);

            result.Should().NotBeNull();
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.Items.Should().HaveCount(2);
            result.Items.First().Name.Should().Be("Alice");
            result.Items.First().Email.Should().Be("alice@x.com");
        }

        [Fact]
        public async Task GetAllAsync_DeveRepassarFiltrosParaORepository()
        {
            var paged = new PagedResult<Lead>(Enumerable.Empty<Lead>(), 0, 2, 5);
            _leadRepoMock
                .Setup(r => r.GetAllAsync("ali", LeadStatus.Qualified, 2, 5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            var result = await _sut.GetAllAsync("ali", LeadStatus.Qualified, 2, 5);

            result.TotalCount.Should().Be(0);
            _leadRepoMock.Verify(r => r.GetAllAsync("ali", LeadStatus.Qualified, 2, 5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_QuandoLeadExiste_DeveRetornarOkComLead()
        {
            var lead = new Lead
            {
                Id = 1,
                Name = "Alice",
                Email = "alice@x.com",
                Status = LeadStatus.New,
                Tasks = new List<TaskItem>
                {
                    new() { Id = 10, LeadId = 1, Title = "T1" }
                }
            };
            _leadRepoMock
                .Setup(r => r.GetByIdWithTasksAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lead);

            var result = await _sut.GetByIdAsync(1);

            result.Success.Should().BeTrue();
            result.Error.Should().BeNull();
            result.Value.Should().NotBeNull();
            result.Value!.Id.Should().Be(1);
            result.Value.Name.Should().Be("Alice");
            result.Value.Tasks.Should().HaveCount(1);
            result.Value.Tasks.First().Title.Should().Be("T1");
        }

        [Fact]
        public async Task GetByIdAsync_QuandoLeadNaoExiste_DeveRetornarFail()
        {
            _leadRepoMock
                .Setup(r => r.GetByIdWithTasksAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lead?)null);

            var result = await _sut.GetByIdAsync(99);

            result.Success.Should().BeFalse();
            result.Value.Should().BeNull();
            result.Error.Should().Contain("99");
        }

        [Fact]
        public async Task CreateAsync_ComDtoValido_DeveCriarERetornarOk()
        {
            var dto = new LeadCreateDto("Alice", "alice@x.com", LeadStatus.New);
            _leadRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lead l, CancellationToken _) =>
                {
                    l.Id = 1;
                    return l;
                });

            var result = await _sut.CreateAsync(dto);

            result.Success.Should().BeTrue();
            result.Value!.Name.Should().Be("Alice");
            result.Value.Email.Should().Be("alice@x.com");
            result.Value.Status.Should().Be(LeadStatus.New);
            _leadRepoMock.Verify(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_QuandoStatusNaoInformado_DeveAssumirNew()
        {
            var dto = new LeadCreateDto("Alice", "alice@x.com", null);
            Lead? captured = null;
            _leadRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
                .Callback((Lead l, CancellationToken _) => captured = l)
                .ReturnsAsync((Lead l, CancellationToken _) => l);

            var result = await _sut.CreateAsync(dto);

            result.Success.Should().BeTrue();
            captured.Should().NotBeNull();
            captured!.Status.Should().Be(LeadStatus.New);
        }

        [Fact]
        public async Task CreateAsync_ComDtoInvalido_DeveLancarValidationExceptionENaoChamarRepo()
        {
            var dto = new LeadCreateDto("Al", "not-an-email", null);

            Func<Task> act = async () => await _sut.CreateAsync(dto);

            await act.Should().ThrowAsync<ValidationException>();
            _leadRepoMock.Verify(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_QuandoLeadExiste_DeveAtualizarECommitar()
        {
            var existing = new Lead { Id = 1, Name = "Old", Email = "old@x.com", Status = LeadStatus.New };
            _leadRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var dto = new LeadUpdateDto("New Name", "new@x.com", LeadStatus.Qualified);

            var result = await _sut.UpdateAsync(1, dto);

            result.Success.Should().BeTrue();
            result.Value!.Name.Should().Be("New Name");
            result.Value.Email.Should().Be("new@x.com");
            result.Value.Status.Should().Be(LeadStatus.Qualified);

            _leadRepoMock.Verify(
                r => r.UpdateAsync(It.Is<Lead>(l => l.Name == "New Name" && l.Email == "new@x.com"),
                                   It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_QuandoLeadNaoExiste_DeveRetornarFailENaoChamarUpdate()
        {
            _leadRepoMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lead?)null);

            var dto = new LeadUpdateDto("XyZ", "x@x.com", LeadStatus.New);

            var result = await _sut.UpdateAsync(99, dto);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("99");
            _leadRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ComDtoInvalido_DeveLancarValidationException()
        {
            var dto = new LeadUpdateDto("Alice", "broken", LeadStatus.New);

            Func<Task> act = async () => await _sut.UpdateAsync(1, dto);

            await act.Should().ThrowAsync<ValidationException>();
            _leadRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _leadRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_QuandoLeadExiste_DeveDeletarERetornarOk()
        {
            var existing = new Lead { Id = 1, Name = "Alice", Email = "alice@x.com" };
            _leadRepoMock
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            var result = await _sut.DeleteAsync(1);

            result.Success.Should().BeTrue();
            _leadRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_QuandoLeadNaoExiste_DeveRetornarFailENaoDeletar()
        {
            _leadRepoMock
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Lead?)null);

            var result = await _sut.DeleteAsync(99);

            result.Success.Should().BeFalse();
            result.Error.Should().Contain("99");
            _leadRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
