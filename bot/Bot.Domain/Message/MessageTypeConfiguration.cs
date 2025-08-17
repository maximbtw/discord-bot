using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Domain.Message;

public class MessageTypeConfiguration : IEntityTypeConfiguration<MessageOrm>
{
    public void Configure(EntityTypeBuilder<MessageOrm> builder)
    {
    }
}