using System.Text.RegularExpressions;
using Application.Common.Results;
using Application.Contracts;

namespace Application.Services;


public class SmsParsingService : ISmsParsingService
{
    private readonly string _amountPattern = @"Ha\s+recibido\s+([\d.,]+)\s+Colones";
    private readonly string _senderPattern = @"Colones\s+de\s+([^p]+?)\s+por";
    private readonly string _descriptionPattern = @",\s+(\d+)\*";
    private readonly string _referencePattern = @"Referencia\s+([0-9*]+)";

    public SmsParsingResult ParseSms(string smsContent)
    {
        var result = new SmsParsingResult { IsValid = false };

        if (string.IsNullOrWhiteSpace(smsContent))
        {
            result.ErrorMessage = "El contenido del SMS está vacío";
            return result;
        }

        try
        {
            var amountResult = ExtractAmount(smsContent);
            if (!amountResult.success)
            {
                result.ErrorMessage = amountResult.error;
                return result;
            }

            var senderResult = ExtractSender(smsContent);
            if (!senderResult.success)
            {
                result.ErrorMessage = senderResult.error;
                return result;
            }

            var description = ExtractDescription(smsContent);

            var referenceResult = ExtractReference(smsContent);
            if (!referenceResult.success)
            {
                result.ErrorMessage = referenceResult.error;
                return result;
            }

            result.IsValid = true;
            result.Amount = amountResult.amount;
            result.SenderName = senderResult.sender;
            result.Description = description;
            result.SinpeReference = referenceResult.reference;

            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error al parsear SMS: {ex.Message}";
            return result;
        }
    }

    private (bool success, decimal amount, string error) ExtractAmount(string smsContent)
    {
        var match = Regex.Match(smsContent, _amountPattern, RegexOptions.IgnoreCase);
        
        if (!match.Success)
        {
            return (false, 0, "No se encontró el monto en el SMS");
        }

        var amountStr = match.Groups[1].Value.Trim();
        
        amountStr = amountStr.Replace(",", "");
        
        if (!decimal.TryParse(amountStr, System.Globalization.CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            return (false, 0, $"El formato del monto no es válido: '{amountStr}'");
        }

        return (true, amount, string.Empty);
    }

    private (bool success, string sender, string error) ExtractSender(string smsContent)
    {
        var match = Regex.Match(smsContent, _senderPattern, RegexOptions.IgnoreCase);
        
        if (!match.Success)
        {
            return (false, string.Empty, "No se encontró el nombre del remitente en el SMS");
        }

        var sender = match.Groups[1].Value.Trim();
        
        if (string.IsNullOrWhiteSpace(sender))
        {
            return (false, string.Empty, "El nombre del remitente está vacío");
        }

        return (true, sender, string.Empty);
    }

    private string? ExtractDescription(string smsContent)
    {
        var match = Regex.Match(smsContent, _descriptionPattern, RegexOptions.IgnoreCase);
        
        return match.Success 
            ? match.Groups[1].Value.Trim() 
            : null;
    }

    private (bool success, string reference, string error) ExtractReference(string smsContent)
    {
        var match = Regex.Match(smsContent, _referencePattern, RegexOptions.IgnoreCase);
        
        if (!match.Success)
        {
            return (false, string.Empty, "No se encontró la referencia SINPE en el SMS");
        }

        var reference = match.Groups[1].Value.Trim();
        
        if (string.IsNullOrWhiteSpace(reference))
        {
            return (false, string.Empty, "La referencia SINPE está vacía");
        }

        return (true, reference, string.Empty);
    }
}
