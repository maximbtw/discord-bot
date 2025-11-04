using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bot.Domain.Orms.ChatSettings;

internal class ChatSettingsTypeConfiguration : IEntityTypeConfiguration<ChatSettingsOrm>
{
    public void Configure(EntityTypeBuilder<ChatSettingsOrm> builder)
    {
        builder.Property(x => x.ChatType)
            .HasConversion(
                v => (byte)v,          
                v => (ChatType)v       
            )
            .IsRequired();
    }
}