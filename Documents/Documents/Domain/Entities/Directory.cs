﻿using YourBrand.Documents.Domain.Common;
using YourBrand.Documents.Domain.Events;

namespace YourBrand.Documents.Domain.Entities;

public class Directory : AuditableEntity, ISoftDelete, IHasDomainEvents, IDeletable, IItem
{
    private readonly HashSet<Document> _documents = new HashSet<Document>();
    private readonly HashSet<Directory> _directories = new HashSet<Directory>();

    private Directory()
    {
    }

    public Directory(string name)
    {
        Id = Guid.NewGuid().ToString();
        Name = name;

        DomainEvents.Add(new DirectoryCreated(Id));
    }

    public string Id { get; private set; } = null!;

    public Directory? Parent { get; private set; }

    public string? ParentId { get; private set; }

    Directory? IItem.Directory => Parent;

    public string Name { get; private set; } = null!;

    public bool Rename(string newName)
    {
        var oldName = Name;

        if (newName != oldName)
        {
            Name = newName;

            DomainEvents.Add(new DirectoryRenamed(Id, newName, oldName));
            return true;
        }

        return false;
    }

    public string? Description { get; private set; } = null!;

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public IReadOnlyCollection<Document> Documents => _documents;

    public void AddDocument(Document document)
    {
        _documents.Add(document);
    }

    public IReadOnlyCollection<Directory> Directories => _directories;

    public Directory CreateDirectory(string name)
    {
        var directory = new Directory(name);
        _directories.Add(directory);
        return directory;
    }

    public List<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();

    public DomainEvent GetDeleteEvent() => new DirectoryDeleted(Id, string.Empty);

    public DateTime? Deleted { get; set; }

    public string? DeletedById { get; set; }
}
