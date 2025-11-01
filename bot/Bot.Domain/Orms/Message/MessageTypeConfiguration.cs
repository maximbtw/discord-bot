using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Domain.Orms.Message;

internal class MessageTypeConfiguration : IEntityTypeConfiguration<MessageOrm>
{
    public void Configure(EntityTypeBuilder<MessageOrm> builder)
    {
    }
}