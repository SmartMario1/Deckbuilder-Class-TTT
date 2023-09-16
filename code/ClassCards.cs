using Sandbox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrorTown;

namespace SmartMario1Classes
{
	public class SpeedCard: ClassCard
	{
		public override string Name => "Speed Up";

		public override string Description => "You become 25% faster";

		public override string LowercaseNospaceName => "speedup";

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var walk = ply.MovementController as TerrorTown.WalkController;
			if ( walk != null )
			{
				walk.SpeedMultiplier *= 1.25f;
			}
		}
	}

	public class SizeCard : ClassCard
	{
		public override string Name => "Size down";

		public override string Description => "You become 25% smaller";

		public override string LowercaseNospaceName => "sizedown";

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			ply.LocalScale *= 0.75f;
			var walk = ply.MovementController as TerrorTown.WalkController;
			if (walk != null )
			{
				walk.SpeedMultiplier /= 0.75f;
			}
		}
	}

	public class FullHeal : ClassCard
	{
		public override string Name => "Full heal";

		public override string Description => "You get all your life back.";

		public override string LowercaseNospaceName => "fullheal";

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			ply.Health = 100f;
		}
	}

	public class BodyArmorCard : ClassCard
	{
		public override string Name => "TANK!!";

		public override string Description => "You get body armor, but you also become 60% larger.";

		public override string LowercaseNospaceName => "bodyarmor";

		public override bool IsUnique => true;

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var armor = new TerrorTown.BodyArmour();
			armor.Touch( ply );
			ply.LocalScale *= 1.6f;
			var walk = ply.MovementController as TerrorTown.WalkController;
			if ( walk != null )
			{
				walk.SpeedMultiplier /= 1.6f;
			}
		}
	}

	public class ExplosionResistCard : ClassCard
	{
		public override string Name => "Explosion Resistance";

		public override string Description => "You get explosion resistance, but also become 15% slower.";

		public override string LowercaseNospaceName => "noexplosion";

		public override bool IsUnique => true;

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var comp = new TTT_Classes.ExplosionResist();
			ply.Components.Add( comp );
			comp.AddIcon();
			var walk = ply.MovementController as TerrorTown.WalkController;
			if ( walk != null )
			{
				walk.SpeedMultiplier *= 0.85f;
			}
		}
	}

	public class RadarCard : ClassCard
	{
		public override string Name => "Terror Sniffer";

		public override string Description => "You get a radar, but you also take 25 damage (traitors be smelly)";

		public override string LowercaseNospaceName => "radarcard";

		public override bool IsUnique => true;

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var radar = new Radar();
			radar.Touch( ply );
			ply.TakeDamage( new DamageInfo { Damage = 25f } );
		}
	}

	public class FartCardComponent : EntityComponent<TerrorTown.Player>
	{
		public RealTimeUntil NextFart = 2;

		private Particles Particle { get; set; }

		[GameEvent.Tick.Server]
		public void OnFartTick()
		{
			if ( NextFart )
			{
				// Following code inspired by the implementation of the discombobulator in TTT by Three Thieves
				Particle = Particles.Create( "particles/discombob_bomb.vpcf" );
				Particle.SetPosition( 0, Entity.Position );
				ParticleCleanupSystem.RegisterForCleanup( Particle );

				Entity.PlaySound( "classfart" );
				foreach ( Entity item in Sandbox.Entity.FindInSphere( Entity.Position, 200f ) )
				{
					if ( item == Entity ) { continue; }
					Vector3 normal = (item.Position - Entity.Position).Normal;
					normal.z = Math.Abs( normal.z ) + 1f;
					item.Velocity += normal * 256f;
					ModelEntity modelEntity = item as ModelEntity;
					if ( modelEntity != null )
					{
						modelEntity.PhysicsBody.Velocity += normal * 50f;
					}

				}
				NextFart = Game.Random.Int( 2, 10 );
			}
		}
	}

	public class FartCard : ClassCard
	{
		public override string Name => "Fart Smella";

		public override string Description => "You start randomly farting every 2-10 seconds";

		public override string LowercaseNospaceName => "fartcard";

		public override bool IsUnique => true;

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var fartcomp = new FartCardComponent();
			ply.Components.Add( fartcomp );
			var farticon = new StatusIcon { IconPath = "/ui/icons/fartcardicon.png" };
			ply.StatusIcons.AddIcon( farticon );
		}

		[Event( "Player.PostOnKilled" )]
		public static void RemoveOnDeath( DamageInfo lastDamage, TerrorTown.Player ply )
		{
			ply.Components.RemoveAny<FartCardComponent>();
		}
	}

	public class InvincibleCardComponent: EntityComponent<TerrorTown.Player>
	{
		public RealTimeSince GotOwner { get; set; }

		private bool haveGotOwner { get; set; } = false;

		public StatusIcon Icon { get; set; }

		[GameEvent.Tick.Server]
		public void OnTick()
		{
			if (!haveGotOwner)
			{
				if (Entity != null)
				{
					GotOwner = 0;
					Icon = new StatusIcon { IconPath = "/ui/icons/invinciblecardicon.png"};
					Entity.StatusIcons.AddIcon( Icon );
					haveGotOwner = true;
				}
				return;
			}
			else if (GotOwner > 6)
			{
				Entity.StatusIcons.RemoveIcon( Icon );
				Entity.Components.Remove( this );
			}
		}

		[Event("Player.PreTakeDamage")]
		public void RemoveDamage(DamageInfo info, TerrorTown.Player ply )
		{
			if (ply == Entity)
			{
				ply.PendingDamage.Damage = 0f;
			}
		}
	}

	public class InvicibleCard: ClassCard
	{
		public override string Name => "Invincibility";

		public override string Description => "You take no damage for 6 seconds after taking this card.";

		public override string LowercaseNospaceName => "invinciblecard";

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var comp = new InvincibleCardComponent();
			ply.Components.Add( comp );
		}
	}

	public class SmartFellaCard: ClassCard
	{
		public override string Name => "Smart Fella";

		public override string Description => "You deduct the identity of a single other alive Innocent.";

		public override string LowercaseNospaceName => "smartfella";

		public override void OnCardAdd( TerrorTown.Player ply )
		{
			var viableplayers = Teams.GetByName( "Innocent" )?.AlivePlayers.Where( x => x != ply );
			if (viableplayers.Count() == 0) 
			{ 
				Chat.AddChatEntry( To.Single( ply ), null, "There are no other Innocent players!" );
				PopupSystem.DisplayPopup( To.Single( ply ), "There are no other Innocent players!" );
				return;
			}
			else
			{
				var randomply = viableplayers.ElementAt(Game.Random.Int(viableplayers.Count() - 1));
				Chat.AddChatEntry( To.Single( ply ), null, randomply.Owner.Name + " is an Innocent!" );
				PopupSystem.DisplayPopup( To.Single( ply ), randomply.Owner.Name + " is an Innocent!" );
			}
		}
	}
}
