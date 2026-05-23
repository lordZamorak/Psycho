package com.ruse.world.content.dialogue.quests;

import com.ruse.model.entity.character.player.Player;

public final class QuestManager {

	private static final int QUEST_COUNT = 2;

	private QuestManager() {
	}

	public static int getQuestCount() {
		return QUEST_COUNT;
	}

	public static int getCompletedQuestCount(Player player) {
		int completed = 0;
		if (isRecipeForDisasterComplete(player)) {
			completed++;
		}
		if (isNomadComplete(player)) {
			completed++;
		}
		return completed;
	}

	public static String getRecipeForDisasterPanelLine(Player player) {
		int wavesCompleted = player.getMinigameAttributes().getRecipeForDisasterAttributes().getWavesCompleted();
		if (isRecipeForDisasterComplete(player)) {
			return "@or2@Recipe for Disaster:  @gre@Complete";
		}
		if (player.getMinigameAttributes().getRecipeForDisasterAttributes().hasFinishedPart(0)) {
			return "@or2@Recipe for Disaster:  @yel@In progress (" + wavesCompleted + "/6)";
		}
		return "@or2@Recipe for Disaster:  @red@Not started";
	}

	public static String getNomadPanelLine(Player player) {
		if (isNomadComplete(player)) {
			return "@or2@Nomad's Requiem:  @gre@Complete";
		}
		if (player.getMinigameAttributes().getNomadAttributes().hasFinishedPart(0)) {
			return "@or2@Nomad's Requiem:  @yel@In progress";
		}
		return "@or2@Nomad's Requiem:  @red@Not started";
	}

	public static String getSummaryPanelLine(Player player) {
		return "@or2@Quest Progress:  @yel@" + getCompletedQuestCount(player) + "/" + getQuestCount();
	}

	private static boolean isRecipeForDisasterComplete(Player player) {
		return player.getMinigameAttributes().getRecipeForDisasterAttributes().getWavesCompleted() >= 6;
	}

	private static boolean isNomadComplete(Player player) {
		return player.getMinigameAttributes().getNomadAttributes().hasFinishedPart(1);
	}
}
