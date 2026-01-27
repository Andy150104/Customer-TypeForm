using BuildingBlocks.CQRS;

namespace ClientService.Application.Forms.Commands.ReorderFields;

public class ReorderFieldsCommand : ICommand<ReorderFieldsCommandResponse>
{
    /// <summary>
    /// Form cần sắp xếp lại thứ tự field
    /// </summary>
    public Guid FormId { get; set; }

    /// <summary>
    /// Danh sách FieldId theo đúng thứ tự mong muốn (từ trên xuống dưới)
    /// </summary>
    public List<Guid> FieldIdsInOrder { get; set; } = new();
}
