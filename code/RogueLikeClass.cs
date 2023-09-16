using Sandbox.Internal;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTT_Classes;
using Editor;

namespace SmartMario1Classes
{
	public abstract class ClassCard
	{
		public abstract string Name { get; }

		public abstract string Description { get; }

		public abstract void OnCardAdd(TerrorTown.Player ply);

		public abstract string LowercaseNospaceName { get; }

		public virtual bool IsUnique { get; set; } = false;

		protected ClassCard() { Roguelike.RegisterCard( this ); }
	}

	public partial class Roguelike : TTT_Class
	{

		public override string Name { get; set; } = "Deckbuilder";
		public override string Description { get; set; } = "You like playing roguelikes! Use your active to pick a card!";
		public override float Frequency { get; set; } = 1f;
		public override Color Color { get; set; } = Color.FromRgb( 0xFF3D67 );

		public override bool hasActiveAbility { get; set; } = true;
		public override float coolDownTimer { get; set; } = 30f;
		public override float buttonDownDuration { get; set; } = 0.2f;

		public static List<ClassCard> Cards = new List<ClassCard>();

		[Net]
		public IList<string> CardNames { get; set; } = new List<string>();

		[Net]
		public bool AllowedToPick { get; set; } = false;

		public override void ActiveAbility()
		{
			AllowedToPick = true;
			Log.Info( "Cardlist:" );
			foreach(var card in Cards)
			{
				Log.Info("Card: " + card.Name  + "; " + card.Description);
			}

			var owner = Entity.Owner as IClient;
			owner.SendCommandToClient( "class_roguelike_class_toggle_ui" );
		}
		public override void RoundStartAbility()
		{
			List<TypeDescription> list = (from type in GlobalGameNamespace.TypeLibrary.GetTypes<ClassCard>()
										  where type.TargetType.IsSubclassOf( typeof( ClassCard ) )
										  select type).ToList();

			if ( list.Count() == 0 )
			{
				Log.Error( "No cards were found." );
				throw new Exception( "No cards were found." );
			}

			foreach ( TypeDescription type in list )
			{
				if ( !Cards.Where( ( ClassCard x ) => x.GetType() == type.TargetType ).Any() )
				{
					type.Create<ClassCard>();
				}
			}

			InitiateCardsClient();
		}

		[ConCmd.Server( "class_roguelike_class_pick_card" )]
		public static void PickCard(string cardName)
		{
			// This security is faulty, because a player can force a card to be picked every time, but I don't think anyone will care enough for it to be a problem.
			var ply = ConsoleSystem.Caller.Pawn as TerrorTown.Player;

			var comp = ply.Components.Get<Roguelike>();

			if ( comp == null ) return;
			if (!comp.AllowedToPick) return;

			ClassCard card = Cards.Where(x => x.LowercaseNospaceName == cardName).FirstOrDefault();
			if ( card == null ) return;
			if ( card.IsUnique && comp.CardNames.Contains( cardName ) ) return;

			Log.Info( "got through card checks, now adding" );
			comp.CardNames.Add( cardName );
			comp.AllowedToPick = false;
			card.OnCardAdd( ply );
		}

		[ConCmd.Client( "class_roguelike_class_toggle_ui" )]
		public static void ToggleUI()
		{
			var ply = ConsoleSystem.Caller.Pawn as TerrorTown.Player;

			var comp = ply.Components.Get<Roguelike>();

			if (comp != null)
			{
				Log.Info( "We got the class" );

				var panel = Game.RootPanel.ChildrenOfType<CardSelect>().FirstOrDefault();
				if (panel == null)
				{
					if (comp.AllowedToPick)
					{
						Log.Info( "pick a card" );
						Game.RootPanel.AddChild<CardSelect>();
						return;
					}
				}
				panel.Delete();
			}
			else
			{
				Log.Info( "Stop trying to cheat!! >:(" );
			}
		}

		public static void RegisterCard(ClassCard card)
		{
			Cards.Add( card );
		}


		[ClientRpc]
		public void InitiateCardsClient()
		{
			List<TypeDescription> list = (from type in GlobalGameNamespace.TypeLibrary.GetTypes<ClassCard>()
										  where type.TargetType.IsSubclassOf( typeof( ClassCard ) )
										  select type).ToList();

			if ( list.Count() == 0 )
			{
				Log.Error( "No cards were found." );
				throw new Exception( "No cards were found." );
			}

			foreach ( TypeDescription type in list )
			{
				if ( !Cards.Where( ( ClassCard x ) => x.GetType() == type.TargetType ).Any() )
				{
					type.Create<ClassCard>();
				}

			}
		}
	}
}
