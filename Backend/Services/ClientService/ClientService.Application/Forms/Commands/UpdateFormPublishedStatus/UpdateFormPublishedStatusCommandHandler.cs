using ClientService.Application.Interfaces.FormServices;
using MediatR;

namespace ClientService.Application.Forms.Commands.UpdateFormPublishedStatus;

public class UpdateFormPublishedStatusCommandHandler : IRequestHandler<UpdateFormPublishedStatusCommand, UpdateFormPublishedStatusCommandResponse>
{
    private readonly IFormService _formService;

    public UpdateFormPublishedStatusCommandHandler(IFormService formService)
    {
        _formService = formService;
    }

    public async Task<UpdateFormPublishedStatusCommandResponse> Handle(UpdateFormPublishedStatusCommand request, CancellationToken cancellationToken)
    {
        return await _formService.UpdateFormPublishedStatusAsync(request, cancellationToken);
    }
}
