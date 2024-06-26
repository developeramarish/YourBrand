﻿using System.Text;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using YourBrand.Documents.Application.Services;
using YourBrand.Documents.Contracts;
using YourBrand.Documents.Domain.Entities;
using YourBrand.Documents.Domain.Enums;
using YourBrand.Documents.Infrastructure.Persistence;

namespace YourBrand.Documents.Consumers;

public class CreateDocumentFromTemplateConsumer(
    ILogger<CreateDocumentFromTemplateConsumer> logger,
    DocumentsContext documentsContext,
    IRazorTemplateCompiler razorTemplateCompiler,
    IPdfGenerator pdfGenerator,
    IMessageDataRepository messageDataRepository) : IConsumer<CreateDocumentFromTemplate>
{
    private readonly IMessageDataRepository _messageDataRepository = messageDataRepository;

    public async Task Consume(ConsumeContext<CreateDocumentFromTemplate> context)
    {
        var request = context.Message;

        DocumentTemplate? documentTemplate = await documentsContext.DocumentTemplates
            .FirstOrDefaultAsync(dt => dt.Id == request.TemplateId, context.CancellationToken);

        if (documentTemplate is null)
        {
            throw new Exception("DocumentTemplate not found");
        }

        string documentContent = await RenderContent(request, documentTemplate);

        byte[] documentBytes = await CreateDocument(request, documentContent);

        await context.RespondAsync<DocumentResponse>(new DocumentResponse(request.DocumentFormat, await _messageDataRepository.PutBytes(documentBytes, TimeSpan.FromDays(1), context.CancellationToken)));
    }

    private async Task<string> RenderContent(CreateDocumentFromTemplate request, DocumentTemplate documentTemplate)
    {
        string documentContent;

        if (documentTemplate.TemplateLanguage == DocumentTemplateLanguage.Razor)
        {
            documentContent = await RenderRazorTemplate(request, documentTemplate);
        }
        else
        {
            documentContent = documentTemplate.Content;
        }

        return documentContent;
    }

    private async Task<string> RenderRazorTemplate(CreateDocumentFromTemplate request, DocumentTemplate documentTemplate)
    {
        dynamic model = ParseModelJson(request.Model);

        string renderedHtml;

        if (!razorTemplateCompiler.HasCachedTemplate(request.TemplateId))
        {
            renderedHtml = await razorTemplateCompiler.CompileAndRenderAsync(
                request.TemplateId,
                documentTemplate.Content,
                model);
        }
        else
        {
            renderedHtml = await razorTemplateCompiler.RenderAsync(
              request.TemplateId,
              model);
        }

        return renderedHtml;
    }

    private dynamic ParseModelJson(string modelJson)
    {
        modelJson = CleanUpJson(modelJson);

        logger.LogDebug($"Model (Json): {modelJson}");

        return JsonConvert.DeserializeObject<object>(modelJson);
    }

    private static string CleanUpJson(string model) => model
            .Replace("\\", string.Empty)
            .Trim('"');

    private async Task<byte[]> CreateDocument(CreateDocumentFromTemplate request, string documentContent)
    {
        byte[] documentBytes;

        if (request.DocumentFormat == DocumentFormat.Html)
        {
            documentBytes = Encoding.UTF8.GetBytes(documentContent);
        }
        else if (request.DocumentFormat == DocumentFormat.Pdf)
        {
            documentBytes = await GeneratePdf(documentContent);
        }
        else
        {
            throw new Exception("Invalid format selected");
        }

        return documentBytes;
    }

    private async Task<byte[]> GeneratePdf(string renderedHtml)
    {
        var pdfStream = await pdfGenerator.GeneratePdfFromHtmlAsync(renderedHtml);
        return await ConvertStreamToBytes(pdfStream);
    }

    private static async Task<byte[]> ConvertStreamToBytes(Stream stream)
    {
        MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream.GetBuffer();
    }
}