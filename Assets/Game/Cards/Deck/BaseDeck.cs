using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------
//----------------------------------------------
// BaseDeck
//----------------------------------------------
//----------------------------------------------
namespace Pebble
{
    public class BaseDeck<CardType> where CardType : BaseCard
    {
        //----------------------------------------------
        // Variables
        private List<CardType> m_cards = null;
        private IDeckOwner m_owner = null;
        //----------------------------------------------
        // Properties
        public int Size
        {
            get
            {
                return m_cards.Count;
            }
        }

        public List<CardType> Cards
        {
            get
            {
                return m_cards;
            }
        }

        public bool Empty
        {
            get
            {
                return Size == 0;
            }
        }

        public IDeckOwner Owner
        {
            get
            {
                return m_owner;
            }
        }


        //------------------------------------------------------
        public BaseDeck(IDeckOwner owner)
        {
            m_owner = owner;
            m_cards = new List<CardType>();
        }

        public BaseDeck() : this(null)
        {

        }

        public List<CardType>.Enumerator GetEnumerator()
        {
            return m_cards.GetEnumerator();
        }

        public void Clear()
        {
            // TODO : Pool and release cards
            m_cards.Clear();
        }

        //------------------------------------------------------
        public void Shuffle()
        {
            m_cards.Shuffle();
            m_cards.Shuffle();
            m_cards.Shuffle();
        }

        //------------------------------------------------------
        public void AddCard(CardType card)
        {
            if (Owner != null) // A deck with no ownership cannot set the new owner
            {
                card.Owner = Owner;
            }
            m_cards.Add(card);
        }

        //------------------------------------------------------
        public void AddCards(List<CardType> cards)
        {
            foreach (CardType card in cards)
            {
                AddCard(card);
            }
        }

        //------------------------------------------------------
        public void RemoveCard(CardType card)
        {
            m_cards.Remove(card);
        }

        //------------------------------------------------------
        public bool Contains(CardType card)
        {
            return m_cards.Contains(card);
        }

        //------------------------------------------------------
        public int MoveCardsTo(int requested, BaseDeck<CardType> other)
        {
            int oldSize = Size;
            if (requested >= oldSize)
            {
                other.AddCards(m_cards);
                m_cards.Clear();
                return oldSize;
            }
            else
            {
                int index = oldSize - requested;
                List<CardType> drawed = m_cards.GetRange(index, requested);
                other.AddCards(drawed);
                m_cards.RemoveRange(index, requested);
                return requested;
            }
        }

        //------------------------------------------------------
        public bool MoveCardTo(CardType card, BaseDeck<CardType> other)
        {
            if (Contains(card))
            {
                other.AddCard(card);
                m_cards.Remove(card);
                return true;
            }
            return false;
        }

        //------------------------------------------------------
        public void MoveAllCardsTo(BaseDeck<CardType> other)
        {
            MoveCardsTo(Size, other);
        }

        public bool IsOverDraw(int requested)
        {
            return requested > Size;
        }

        public void CopyFrom(BaseDeck<CardType> other)
        {
            m_owner = other.Owner;
            AddCards(other.Cards);
        }

        public void Print(string prefix)
        {
            string toPrint = prefix + " : ";
            if (Cards.Count > 0)
            {
                foreach (CardType card in Cards)
                {
                    toPrint += card.ToString() + ", ";
                }
            }
            else
            {
                toPrint += "No Cards";
            }

            Debug.Log(toPrint);
        }
    }
}