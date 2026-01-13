using ClientService.Application.Interfaces.FormServices;
using MediatR;

namespace ClientService.Application.Forms.Commands.SubmitForm;

public class SubmitFormCommandHandler : IRequestHandler<SubmitFormCommand, SubmitFormCommandResponse>
{
    private readonly IFormService _formService;

    public SubmitFormCommandHandler(IFormService formService)
    {
        _formService = formService;
    }

    public async Task<SubmitFormCommandResponse> Handle(SubmitFormCommand request, CancellationToken cancellationToken)
    {
        return await _formService.SubmitFormAsync(request, cancellationToken);
    }
}
