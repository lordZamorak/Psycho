package com.ruse.world.content.skill;

import com.ruse.model.definitions.ItemDefinition;
import com.ruse.model.entity.character.player.Player;
import com.ruse.util.Misc;

public final class SkillRequirementMessages {

	private SkillRequirementMessages() {
	}

	public static void missingWoodcuttingAxe(Player player) {
		missingRequirement(player, "a woodcutting axe/hatchet");
	}

	public static void missingMiningPickaxe(Player player) {
		missingRequirement(player, "a mining pickaxe");
	}

	public static void missingItem(Player player, int itemId) {
		String name = itemName(itemId);
		missingRequirement(player, Misc.anOrA(name) + " " + name);
	}

	public static void missingRequirement(Player player, String requirement) {
		player.getPacketSender().sendMessage("You do not have " + requirement + ".");
	}

	public static void doesNotMeet(Player player, String action) {
		player.getPacketSender().sendMessage("Player does not meet requirements to " + action + ".");
	}

	public static String itemName(int itemId) {
		ItemDefinition definition = ItemDefinition.forId(itemId);
		if (definition == null || definition.getName() == null) {
			return "item " + itemId;
		}
		return definition.getName().toLowerCase().replace("_", " ");
	}
}
