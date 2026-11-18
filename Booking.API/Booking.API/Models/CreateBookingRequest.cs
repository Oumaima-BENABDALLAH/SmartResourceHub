using MediatR;
using System.Text.Json.Serialization;

namespace Booking.API.Models
{
    //CreateBookingRequest  un objet (un record) qui sert de passerelle entre le client (front-end) et ton application. Il récupère les données HTTP de la réservation.
    //Il transporte ces données et fait office de commande MediatR pour déclencher l'action de création d'une réservation dans le système,
    //sans retourner de données particulières (grâce à IRequest<Unit>).
    public sealed record CreateBookingRequest(
    [property: JsonPropertyName("resourceId")] Guid ResourceId,// Permet de mapper la clé JSON "resourceId": "d3b07384-d113-4ec7-97d8-55088c421715"  vers la propriété C# ResourceId (en PascalCase)
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("startTime")] DateTime StartTime,
    [property: JsonPropertyName("endTime")] DateTime EndTime) : IRequest<Unit>;// Indique à MediatR qu'il s'agit d'une commande qui exécute une action sans retourner de données (Unit équivalent de void)

}
