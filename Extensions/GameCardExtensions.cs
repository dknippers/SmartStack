using UnityEngine;

namespace ReverseStack.Extensions;
public static class GameCardExtensions
{
    /// <summary>
    /// Indicates if this stack can have the other stack on top, i.e. if the root of <paramref name="other"/> can be placed on the leaf of <paramref name="card"/>.
    /// This is similar to the built-in <see cref="CardData.CanHaveCardOnTop(CardData, bool)"/> except we also consider status cards in the stack of <paramref name="card"/>,
    /// which the game implements in a separate method <see cref="CardData.CanHaveCardsWhileHasStatus()"/>.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">The other card / stack</param>
    /// <returns></returns>
    public static bool CanHaveOnTop(this GameCard card, GameCard other)
    {
        var leaf = card.GetLeafCard();
        var root = other.GetRootCard();

        if (!leaf.CardData.CanHaveCardOnTop(root.CardData))
        {
            return false;
        }

        // The CanHaveCardOnTop method does not check for status cards for some reason
        // so we perform that check here to ensure `card` allows `other` to be placed on top of it.
        if (card.GetCardWithStatusInStack() is GameCard statusCard &&
           !statusCard.CardData.CanHaveCardsWhileHasStatus())
        {
            // Certain cards do not allow any cards to be placed on them when any card in the stack
            // has a status effect running right now.
            return false;
        }

        return true;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> and <paramref name="other"/> belong to the same stack of cards.
    /// </summary>
    /// <param name="card">This card / stack</param>
    /// <param name="other">Other card / stack</param>
    /// <returns></returns>
    public static bool IsSameStack(this GameCard card, GameCard other)
    {
        if (ReferenceEquals(card, other))
        {
            return true;
        }

        var r1 = card.GetRootCard();
        var r2 = other.GetRootCard();

        if (ReferenceEquals(r1, r2))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> allows a Reverse Stack operation with <paramref name="other"/>,
    /// i.e. whether <paramref name="other"/> can be stacked on top of <paramref name="card"/>.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    public static bool CanReverseStackOn(this GameCard card, GameCard other)
    {
        return
            !other.IsEquipped &&
            other.Combatable?.InConflict != true &&
            !card.IsSameStack(other) &&
            card.CanHaveOnTop(other);
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> allows an automatic Reverse Stack operation with <paramref name="other"/>,
    /// i.e. whether <paramref name="other"/> can be stacked on top of <paramref name="card"/> without the user performing a drag operation.
    /// This occurs for example when a Card is produced by a <see cref="Harvestable"/> or created in some other way.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    public static bool CanAutoReverseStackOn(this GameCard card, GameCard other)
    {
        // We only allow automatic Reverse Stack when the cards are the same kind (e.g. both an Apple Tree)
        // This is similar to what the game checks when Cards are bouncing into other cards.
        return card.IsSamePrefab(other) && card.CanReverseStackOn(other);
    }

    /// <summary>
    /// Instantly sets the position of <paramref name="card"/> to the given <paramref name="position"/>.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="position">Position</param>
    public static void SetPosition(this GameCard card, Vector3 position)
    {
        // Both the underlying transform position AND the custom TargetPosition
        // properties need to be set to instantly set a card's position correctly.
        card.transform.position = card.TargetPosition = position;
    }

    /// <summary>
    /// Indicates if <paramref name="card"/> and <paramref name="other"/> are instances of the same prefab.
    /// </summary>
    /// <param name="card">Card</param>
    /// <param name="other">Other card</param>
    /// <returns></returns>
    public static bool IsSamePrefab(this GameCard card, GameCard other)
    {
        if (ReferenceEquals(card, other)) return true;

        return card.CardData.Id == other.CardData.Id;
    }

    public static bool IsRoot(this GameCard card) => card.Parent is null;
    public static bool IsLeaf(this GameCard card) => card.Child is null;
}
