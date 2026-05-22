package org.necrotic.client.cache.definition;

public class ObjectDefinitionCustom {

    public static boolean load377(int id) {
        switch (id) {
            case 80822: // yew tree
            case 71279: // normal tree
            case 71278: // normal tree
            case 71276: // normal tree
            case 80820: // normal tree
                return true;
            default:
                return false;
        }
    }

    private static final int[] showBlack = { 3735, 26346, 26347, 26348, 26358,
            26359, 26360, 26361, 26362, 26363, 26364 };

    public static boolean load667(int id) {
        return
                /*id == 8550 || id == 8551 || id == 7847 || id == 8150 || */id == 32159 || id == 32157 || id == 36672 || id == 36675
                || id == 36692 || id == 34138 || id >= 39260 && id <= 39271 || id == 39229 || id == 39230 || id == 39231 || id == 36676 || id == 36692
                || id > 11915 && id <= 11929 || id >= 11426 && id <= 11444 || id >= 14835 && id <= 14845 || id >= 11391 && id <= 11397 || id >= 12713 && id <= 12715;
    }

    public static void do667(ObjectDefinition definition) {
        for (int element : showBlack) {
            if (definition.type == element) {
                definition.modifiedModelColors = new int[1];
                definition.originalModelColors = new int[1];
                definition.modifiedModelColors[0] = 0;
                definition.originalModelColors[0] = 1;
            }

        }

        if(definition.name == null) {
            definition.name = "";
            definition.hasActions = false;
        }

        int[][] shootingStars = { { 38661, 42165 }, { 38662, 42166 },
                { 38663, 42163 }, { 38664, 42164 }, { 38665, 42160 },
                { 38666, 42159 }, { 38667, 42168 }, { 38668, 42169 }, };

        for (int[] i : shootingStars) {
            if (definition.type == i[0]) {
                ObjectDefinition.stream[1].position = ObjectDefinition.streamIndices[1][3514];
                definition.setDefaults();
                definition.readValues(ObjectDefinition.stream[1]);
                definition.objectModelIDs = new int[1];
                definition.objectModelIDs[0] = i[1];
                definition.sizeX = 2;
                definition.sizeY = 2;
                definition.name = "Crashed star";
                definition.actions = new String[5];
                definition.actions[0] = "Mine";
                definition.description = "A crashed star!".getBytes();
            }
        }
        loadEvilTree(definition);

        if (definition.actions == null || definition.actions.length < 5) {
            String[] newActions = new String[5];
            if(definition.actions != null) {
                for (int i = 0; i < 5; i++) {
                    if (i >= definition.actions.length) {
                        newActions[i] = null;
                    } else {
                        newActions[i] = definition.actions[i];
                    }
                }
            }
            definition.actions = newActions;
        }


        boolean removeObject = definition.type == 1442 || definition.type == 1433 || definition.type == 1443 || definition.type == 1441 || definition.type == 26916 || definition.type == 26917 || definition.type == 5244 || definition.type == 2623 || definition.type == 2956 || definition.type == 463 || definition.type == 462 || definition.type == 10527 || definition.type == 10529 || definition.type == 40257 || definition.type == 296 || definition.type == 300 || definition.type == 1747 || definition.type == 7332 || definition.type == 7326 || definition.type == 7325 || definition.type == 7385 || definition.type == 7331 || definition.type == 7385 || definition.type == 7320 || definition.type == 7317 || definition.type == 7323 || definition.type == 7354 || definition.type == 1536 || definition.type == 1537 || definition.type == 5126 || definition.type == 1551 || definition.type == 1553 || definition.type == 1516 || definition.type == 1519 || definition.type == 1557 || definition.type == 1558 || definition.type == 7126 || definition.type == 733 || definition.type == 14233 || definition.type == 14235 || definition.type == 1596 || definition.type == 1597 || definition.type == 14751 || definition.type == 14752 || definition.type == 14923 || definition.type == 36844 || definition.type == 30864 || definition.type == 2514 || definition.type == 1805 || definition.type == 15536 || definition.type == 2399 || definition.type == 14749 || definition.type == 29315 || definition.type == 29316 || definition.type == 29319 || definition.type == 29320 || definition.type == 29360 || definition.type == 1528 || definition.type == 36913 || definition.type == 36915 || definition.type == 15516 || definition.type == 35549 || definition.type == 35551 || definition.type == 26808 || definition.type == 26910 || definition.type == 26913 || definition.type == 24381 || definition.type == 15514 || definition.type == 25891 || definition.type == 26082 || definition.type == 26081 || definition.type == 1530 || definition.type == 16776 || definition.type == 16778 || definition.type == 28589 || definition.type == 1533 || definition.type == 17089 || definition.type == 1600 || definition.type == 1601 || definition.type == 11707 || definition.type == 24376 || definition.type == 24378 || definition.type == 40108 || definition.type == 59 || definition.type == 2069 || definition.type == 36846 || definition.type == 1506 ||
                definition.type == 9299 ||
                definition.type == 1508 || definition.type  == 4031 || definition.type == 11470 || definition.name != null && ((definition.name.equalsIgnoreCase("door") && definition.type != 15644 && definition.type != 15641) || definition.name.equalsIgnoreCase("gate"));
        if(removeObject) {
            definition.objectModelIDs = null;
            definition.hasActions = false;
            definition.isUnwalkable = false;
            return;
        }
        /*if(definition.varbitIndex <= 484 && definition.varbitIndex >= 469) {
            definition.configID = definition.varbitIndex;
            definition.varbitIndex = -1;
        }*/
        if(definition.name != null && definition.type != 591) {
            String s =definition.name.toLowerCase();
            if(s.contains("bank") && !s.contains("closed")) {
                definition.actions = new String[5];
                definition.actions[0] = "Use";
                definition.actions[1] = "Use Quickly";
            }
        }
        if (definition.type == 2882 || definition.type == 2883) {
            definition.name = "Gate";
            definition.actions = new String[5];
            definition.actions[0] = "Open";
            definition.actions[1] = null;
        }
        if (definition.type == 28295) {
            definition.name = "@red@Holiday snowman";
            definition.actions[0] = "@gre@Teleport to event";
        }
        if (definition.type == 11666) {
            definition.actions = new String[5];
            definition.actions[0] = "Smelt";
            definition.actions[1] = "Craft";
        }
        if (definition.type == 28426 || definition.type == 28427) {
            definition.name = "Rocky plinth";
            definition.actions = new String[5];
            definition.description = "A very old rock formation.".getBytes();
        }
        if (definition.type == 28449 || definition.type == 28474 || definition.type == 28448) {
            definition.name = "Dense brush";
            definition.actions = new String[5];
            definition.description = "Too thick to walk through.".getBytes();
        }
        if (definition.type == 28457 ||
                definition.type == 28458 ||
                definition.type == 28459 ||
                definition.type == 28460 ||
                definition.type == 28461 ||
                definition.type == 28462 ||
                definition.type == 28463 ||
                definition.type == 28464 ||
                definition.type == 28465 ||
                definition.type == 28466 ||
                definition.type == 28467 ||
                definition.type == 28468 ||
                definition.type == 28469 ||
                definition.type == 28470 ||
                definition.type == 28471 ||
                definition.type == 28472 ||
                definition.type == 28473 ||
                definition.type == 28456) {
            definition.name = "Murky water";
            definition.description = "This water doesn't look clean...".getBytes();
        }
        if (definition.type == 134 || definition.type == 135) {
            definition.name = "Heavy door";
            definition.actions = new String[5];
            definition.actions[0] = "Push";
        }
        if (definition.type == 17953) { //zulrah boat
            definition.actions = new String[5];
            definition.actions[0] = "Board";
            definition.description = "I want you to know that I was rooting for you, mate. Know that.".getBytes();
        }
        if (definition.type == 57225) {
            definition.actions = new String[5];
            definition.actions[0] = "Climb-over";
        }
        if (definition.type == 2305) {
            definition.actions = new String[5];
            definition.actions[0] = "Escape";
        }
        if (definition.type == 589) {
            definition.actions = new String[5];
            definition.actions[0] = "Search";
        }
        if (definition.type == 11678) {
            definition.actions = new String[5];
            definition.actions[0] = "Inspect";
        }
        if (definition.type == 11339) {
            definition.actions = new String[5];
            definition.actions[0] = "Search";
        }
        if (definition.type == 5595) {
            definition.actions = new String[5];
            definition.actions[0] = "Search";
        }
        if (definition.type == 2725) {
            definition.actions = new String[5];
            definition.actions[0] = "Search";
        }
        if (definition.type == 423) {
            definition.actions = new String[5];
            definition.actions[0] = "Search";
        }
        if (definition.type == 57258) {
            definition.actions = new String[5];
            definition.actions[0] = "Climb";
        }
        if (definition.type == 1739) {
            definition.actions = new String[5];
            definition.actions[0] = "Climb-up";
            definition.actions[1] = "Climb-down";
            //definition.actions[4] = "Climb-down";
        }
        if (definition.type == 11698) {
            definition.name = null;
            definition.hasActions = false;
            definition.actions = null;
            definition.modifiedModelColors = new int[] {6817, 6697, 6693, 7580};
            definition.originalModelColors = new int[] {21543, 21547, 45, 7341};
            definition.objectModelIDs = new int[] { 5013 };
        }
        if (definition.type == 11699) {
            definition.name = null;
            definition.hasActions = false;
            definition.actions = null;
            definition.modifiedModelColors = new int[] {74, 43117};
            definition.originalModelColors = new int[] {21543, 21547};
            definition.objectModelIDs = new int[] { 1424 };
        }
        if(definition.type == 5259) {
            definition.actions = new String[5];
            definition.name = "Ghost Town Barrier";
            definition.actions[0] = "Pass";
        }
        if(definition.type == 10805 || definition.type == 10806 || definition.type == 10807) {
            definition.name = "Grand Exchange clerk";
            definition.hasActions = true;
            definition.actions = new String[5];
            definition.actions[0] = "Use";
        }
        if(definition.type == 10091) {
            definition.actions = new String[] {"Bait", null, null, null, null};
            definition.name = "@yel@Rocktail fishing spot";
        }
        if(definition.type == 7836 || definition.type == 7808) {
            definition.hasActions = true;
            definition.actions = new String[] {"Dump-weeds", null, null, null, null};
            definition.name = "Compost bin";
        }
        if(definition.type == 26945) {
            definition.actions = new String[] {"Investigate", "Contribute", null, null, null};
            definition.name = "Well of Goodwill";
        }
        if(definition.type == 25014 || definition.type == 25026 || definition.type == 25020 || definition.type == 25019 || definition.type == 25024 || definition.type == 25025 || definition.type == 25016 || definition.type == 5167 || definition.type == 5168) {
            definition.actions = new String[5];
        }
        if(definition.type == 1948) {
            definition.name = "Wall";
        }
        if(definition.type == 25029) {
            definition.actions = new String[5];
            definition.actions[0] = "Go-through";
        }
        if(definition.type == 19187 || definition.type == 19175) {
            definition.actions = new String[5];
            definition.actions[0] = "Dismantle";
        }
        if(definition.type == 6434) {
            definition.actions = new String[5];
            definition.actions[0] = "Enter";
        }
        if(definition.type == 2182) {
            definition.actions = new String[5];
            definition.actions[0] = "Buy-Items";
            definition.name = "Culinaromancer's chest";
        }
        if(definition.type == 10177)
        {
            definition.actions = new String[5];
            definition.actions[0] = "Climb-down";
            definition.actions[1] = "Climb-up";
        }
        if(definition.type == 39515)
        {
            definition.name = "Pvm Portal";
        }
        if(definition.type == 2026)
        {
            definition.actions = new String[5];
            definition.actions[0] = "Net";
        }
        if(definition.type == 2029)
        {
            definition.actions = new String[5];
            definition.actions[0] = "Lure";
            definition.actions[1] = "Bait";
        }
        if(definition.type == 2030)
        {
            definition.actions = new String[5];
            definition.actions[0] = "Cage";
            definition.actions[1] = "Harpoon";
        }
        if(definition.type == 7352) {
            definition.name = "Gatestone portal";
            definition.actions = new String[5];
            definition.actions[0] = "Enter";
        }
        if(definition.type == 11356)
        {
            definition.name = "Dark Beast Portal";
        }
        if(definition.type == 47120) {
            definition.name = "Altar";
            definition.actions = new String[5];
            definition.actions[0] = "Craft-rune";
        }
        if(definition.type == 11325 || definition.type == 11328|| definition.type == 37943 || definition.type == 37940
                || definition.type == 11325) {
            definition.hasActions = false;
        }
        if(definition.type == 20331)
        {
            definition.actions = new String[5];
            definition.actions[0] = "Steal-from";
        }
        if(definition.type == 47180)
        {
            definition.name = "Frost Dragon Portal Device";
            definition.actions = new String[5];
            definition.actions[0] = "Activate";
        }
        if(definition.type == 8702) {
            definition.name = "Rocktail Barrel";
            definition.actions = new String[5];
            definition.actions[0] = "Fish-from";
        }
        if(definition.type == 2783)
        {
            definition.hasActions = true;
            definition.name = "Anvil";
            definition.actions = new String[5];
            definition.actions[0] = "Smith-on";
        }
        if(definition.type == 172) {
            definition.name = "Crystal Chest";
        }
        if(definition.type == 6714)
        {
            definition.hasActions = true;
            definition.name = "Door";
            definition.actions[0] = "Open";
        }
        if(definition.type == 8550 || definition.type == 8150 || definition.type == 8551 || definition.type == 7847 || definition.type == 8550)
        {
            definition.actions = new String[] {null, "Inspect", null, "Guide", null};

            definition.hasActions = true;

        }
        if(definition.type == 42151 || definition.type == 42160)
        {
            definition.name = "Rocks";
            definition.hasActions = true;
            definition.mapSceneID = 11;
        }
        if(definition.type == 42158 || definition.type == 42157)
        {
            definition.name = "Rocks";
            definition.hasActions = true;
            definition.mapSceneID = 12;
        }
        if(definition.type == 42123 || definition.type == 42124 || definition.type == 42119 || definition.type == 42120 || definition.type == 42118 || definition.type == 42122)
        {
            definition.name = "Tree";
            definition.hasActions = true;
            definition.actions = new String[] {"Cut", null, null, null, null};
            definition.mapSceneID = 0;
        }
        if(definition.type == 42127 || definition.type == 42131 || definition.type == 42133 || definition.type == 42129 || definition.type == 42134)
        {
            definition.name = "Tree";
            definition.hasActions = true;
            definition.actions = new String[] {"Cut", null, null, null, null};
            definition.mapSceneID = 6;
        }
        if(definition.type == 42082 || definition.type == 42083)
            definition.mapSceneID = 0;
        if(definition.type >= 42087 && definition.type <= 42117)
            definition.mapSceneID = 4;
        if(definition.type > 30000 && definition.name != null && definition.name.toLowerCase().contains("gravestone"))
            definition.mapSceneID = 34;
        if(definition.type == 36676)
        {
            definition.objectModelIDs = new int[]{17374, 17383};
            //    definition.objectModelTypes = null;
        }
        if(definition.type == 34255)
        {
            definition.configID = 8002;
            definition.configObjectIDs = new int[]
                    {
                            15385
                    };
        }
        if(definition.type == 13830)
        {
            //definition.objectModelIDs = new int[] {12199};
            definition.configID = 8003;
            definition.configObjectIDs = new int[]
                    {
                            13217, 13218, 13219, 13220, 13221, 13222, 13223
                    };
        }
        if (definition.type == 21634) {
            definition.hasActions = true;
            definition.actions = new String[5];
            definition.actions[0] = "Sail";
        }
        if (definition.type == 10284) {
            definition.name = "Chest";
            definition.hasActions = true;
            definition.actions = new String[5];
            definition.actions[0] = "Open";
        }
        if (definition.type == 22721) {
            definition.hasActions = true;
            definition.actions = new String[5];
            definition.actions[0] = "Smelt";
        }
        if (definition.type == 7837) {
            definition.hasActions = true;
            definition.actions = new String[5];
        }
        if (definition.type == 26280){
            definition.hasActions = true;
            definition.actions = new String[5];
            definition.actions[0] = "Study";
        }
        if (definition.type == 27339 || definition.type == 27306)
        {
            definition.hasActions = true;
            definition.name = "Mystical Monolith";
            definition.actions = new String[5];
            definition.actions[0] = "Travel";
            definition.actions[1] = "Pray-at";
        }
        if(definition.type == 15314 || definition.type == 15313)
        {
            definition.configID = 8000;
            definition.configObjectIDs = new int[] {definition.type, -1};
        }
        if(definition.type == 15306)
        {
            definition.configID = 8001;
            definition.configObjectIDs = new int[] {definition.type,-1, 13015};
        }
        if(definition.type == 15305)
        {
            definition.configID = 8001;
            definition.configObjectIDs = new int[] {definition.type, -1, 13016};
        }
        if(definition.type == 15317)
        {
            definition.configID = 8001;
            definition.configObjectIDs = new int[] {definition.type, -1, 13096};
        }
        if(definition.type == 8550)
        {
            definition.configObjectIDs = new int[]
                    {
                            8576, 8575, 8574, 8573, 8576, 8576, 8558, 8559, 8560, 8561, 8562, 8562, 8562, 8580, 8581, 8582, 8583, 8584, 8584, 8584, 8535, 8536, 8537, 8538, 8539, 8539, 8539, 8641, 8642, 8643, 8644, 8645, 8645, 8645, 8618, 8619, 8620, 8621, 8622, 8623, 8624, 8624, 8624, 8595, 8596, 8597, 8598, 8599, 8600, 8601, 8601, 8601, 8656, 8657, 8658, 8659, 8660, 8661, 8662, 8663, 8664, 8664, 8664, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8563, 8564, 8565, 8566, 8576, 8576, 8576, 8585, 8586, 8587, 8588, 8576, 8576, 8576, 8540, 8541, 8542, 8543, 8576, 8576, 8576, 8646, 8647, 8648, 8649, 8576, 8576, 8576, 8625, 8626, 8627, 8628, 8629, 8630, 8576, 8576, 8576, 8602, 8603, 8604, 8605, 8606, 8607, 8576, 8576, 8576, 8665, 8666, 8667, 8668, 8669, 8670, 8671, 8672, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8567, 8568, 8569, 8576, 8576, 8576, 8576, 8589, 8590, 8591, 8576, 8576, 8576, 8576, 8544, 8545, 8546, 8576, 8576, 8576, 8576, 8650, 8651, 8652, 8576, 8576, 8576, 8576, 8631, 8632, 8633, 8634, 8635, 8576, 8576, 8576, 8576, 8608, 8609, 8610, 8611, 8612, 8576, 8576, 8576, 8576, 8673, 8674, 8675, 8676, 8677, 8678, 8679, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8570, 8571, 8572, 8576, 8576, 8576, 8576, 8592, 8593, 8594, 8576, 8576, 8576, 8576, 8547, 8548, 8549, 8576, 8576, 8576, 8576, 8653, 8654, 8655, 8576, 8576, 8576, 8576, 8636, 8637, 8638, 8639, 8640, 8576, 8576, 8576, 8576, 8613, 8614, 8615, 8616, 8617, 8576, 8576, 8576, 8576, 8680, 8681, 8682, 8683, 8684, 8685, 8686, 8576, 8576, 8576, 8576
                    };
        }
        if(definition.type == 8551)
        {
            definition.configObjectIDs = new int[]
                    {
                            8576, 8575, 8574, 8573, 8576, 8576, 8558, 8559, 8560, 8561, 8562, 8562, 8562, 8580, 8581, 8582, 8583, 8584, 8584, 8584, 8535, 8536, 8537, 8538, 8539, 8539, 8539, 8641, 8642, 8643, 8644, 8645, 8645, 8645, 8618, 8619, 8620, 8621, 8622, 8623, 8624, 8624, 8624, 8595, 8596, 8597, 8598, 8599, 8600, 8601, 8601, 8601, 8656, 8657, 8658, 8659, 8660, 8661, 8662, 8663, 8664, 8664, 8664, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8563, 8564, 8565, 8566, 8576, 8576, 8576, 8585, 8586, 8587, 8588, 8576, 8576, 8576, 8540, 8541, 8542, 8543, 8576, 8576, 8576, 8646, 8647, 8648, 8649, 8576, 8576, 8576, 8625, 8626, 8627, 8628, 8629, 8630, 8576, 8576, 8576, 8602, 8603, 8604, 8605, 8606, 8607, 8576, 8576, 8576, 8665, 8666, 8667, 8668, 8669, 8670, 8671, 8672, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8567, 8568, 8569, 8576, 8576, 8576, 8576, 8589, 8590, 8591, 8576, 8576, 8576, 8576, 8544, 8545, 8546, 8576, 8576, 8576, 8576, 8650, 8651, 8652, 8576, 8576, 8576, 8576, 8631, 8632, 8633, 8634, 8635, 8576, 8576, 8576, 8576, 8608, 8609, 8610, 8611, 8612, 8576, 8576, 8576, 8576, 8673, 8674, 8675, 8676, 8677, 8678, 8679, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8576, 8570, 8571, 8572, 8576, 8576, 8576, 8576, 8592, 8593, 8594, 8576, 8576, 8576, 8576, 8547, 8548, 8549, 8576, 8576, 8576, 8576, 8653, 8654, 8655, 8576, 8576, 8576, 8576, 8636, 8637, 8638, 8639, 8640, 8576, 8576, 8576, 8576, 8613, 8614, 8615, 8616, 8617, 8576, 8576, 8576, 8576, 8680, 8681, 8682, 8683, 8684, 8685, 8686, 8576, 8576, 8576, 8576
                    };
        }
        if(definition.type == 7847)
        {
            definition.configObjectIDs = new int[]
                    {
                            7843, 7842, 7841, 7840, 7843, 7843, 7843, 7843, 7867, 7868, 7869, 7870, 7871, 7899, 7900, 7901, 7902, 7903, 7883, 7884, 7885, 7886, 7887, 7919, 7920, 7921, 7922, 7923, 7851, 7852, 7853, 7854, 7855, 7918, 7917, 7916, 7915, 41538, 41539, 41540, 41541, 41542, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7872, 7873, 7874, 7875, 7843, 7904, 7905, 7906, 7907, 7843, 7888, 7889, 7890, 7891, 7843, 7924, 7925, 7926, 7927, 7843, 7856, 7857, 7858, 7859, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7876, 7877, 7878, 7843, 7843, 7908, 7909, 7910, 7843, 7843, 7892, 7893, 7894, 7843, 7843, 7928, 7929, 7930, 7843, 7843, 7860, 7861, 7862, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7879, 7880, 7881, 7882, 7843, 7911, 7912, 7913, 7914, 7843, 7895, 7896, 7897, 7898, 7843, 7931, 7932, 7933, 7934, 7843, 7863, 7864, 7865, 7866, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843, 7843
                    };
        }
        if(definition.type == 8150)
        {
            definition.configObjectIDs = new int[]
                    {
                            8135, 8134, 8133, 8132, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 21101, 21127, 21159, 21178, 21185, 21185, 21185, 17776, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 17777, 17778, 17780, 17781, 17781, 17781, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8144, 8145, 8146, 8147, 8148, 8149, 8144, 8145, 8146, 8144, 8145, 8146, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 9044, 9045, 9046, 9047, 9048, 9048, 9049, 9050, 9051, 9052, 9053, 9054, 8139, 8140, 8141, 8142, 8143, 8143, 8143, 8144, 8145, 8146, 8135, 8135, 8135, 8135, 8135, 8135, -1, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135, 8135
                    };
        }

        switch (definition.type) {
            case 2470:
                definition.name = "@red@Event Portal";
                break;
            case 38700:
                definition.name = "Free-For-All Portal @gre@(SAFE)";
                break;
            case 6725:
            case 6714:
            case 6734:
            case 6730:
            case 6749:
            case 6742:
            case 6723:
            case 6728:
            case 6747:
            case 6744:
            case 6741:
            case 6779:
            case 6743:
            case 6719:
            case 6717:
            case 6731:
            case 6716:
            case 6720:
            case 6738:
            case 6726:
            case 6740:
            case 6721:
            case 6748:
            case 6729:
            case 6745:
            case 6718:
            case 6780:
            case 6746:
            case 6750:
            case 6722:
            case 6715:
                definition.name = "Door";
                definition.hasActions = true;
                break;
            case 5917:
                definition.actions = new String[5];
                definition.name = "Plasma Vent"; //friday the 13th
                definition.actions[1] = "Search";
                break;
            case 4875:
                definition.name = "Banana Stall";
                break;
            case 4874:
                definition.name = "Ring Stall";
                break;
            case 13493:
                definition.actions = new String[5];
                definition.actions[0] = "Steal from";
                break;
            case 2152:
                definition.actions = new String[5];
                definition.actions[0] = "Infuse Pouches";
                definition.actions[1] = "Renew Points";
                definition.name = "Summoning Obelisk";
                break;
            case 4306:
                definition.actions = new String[5];
                definition.actions[0] = "Use";
                break;
            case 2732:
            case 11404:
            case 11406:
            case 11405:
            case 20000:
            case 20001:
                definition.actions = new String[5];
                definition.actions[0] = "Add logs";
                break;
            case 2:
                definition.name = "Entrance";
                break;
        }
    }

    public static void loadEvilTree(ObjectDefinition definition) {
        switch (definition.type) {

            case 11391:
                definition.objectModelIDs = new int[] { 45733, 45735 };
                definition.anIntArray776 = null;
                definition.name = "Seedling";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1694;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Nurture", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11392:
                definition.objectModelIDs = new int[] { 45733, 45731, 45735 };
                definition.anIntArray776 = null;
                definition.name = "Sapling";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1695;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Nurture", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11394:
                definition.objectModelIDs = new int[] { 45736, 45739, 45735 };
                definition.anIntArray776 = null;
                definition.name = "Young tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1697;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Nurture", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11395:
                definition.objectModelIDs = new int[] { 45739, 45741, 45735 };
                definition.anIntArray776 = null;
                definition.name = "Young tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1698;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Nurture", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 12713:
                definition.objectModelIDs = new int[] { 45759 };
                definition.anIntArray776 = null;
                definition.name = "Fallen tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = -1;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = null;
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 12714:
                definition.objectModelIDs = new int[] { 45754 };
                definition.anIntArray776 = null;
                definition.name = "Fallen tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = -1;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = null;
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 12715:
                definition.objectModelIDs = new int[] { 45752 };
                definition.anIntArray776 = null;
                definition.name = "Fallen tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = -1;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = null;
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11922:
                definition.objectModelIDs = new int[] { 45748 };
                definition.anIntArray776 = null;
                definition.name = "Elder evil tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1134;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11919:
                definition.objectModelIDs = new int[] { 45750 };
                definition.anIntArray776 = null;
                definition.name = "Evil magic tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1679;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11916:
                definition.objectModelIDs = new int[] { 45757 };
                definition.anIntArray776 = null;
                definition.name = "Evil yew tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1685;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11444:
                definition.objectModelIDs = new int[] { 45745 };
                definition.anIntArray776 = null;
                definition.name = "Evil maple tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1682;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11441:
                definition.objectModelIDs = new int[] { 45762 };
                definition.anIntArray776 = null;
                definition.name = "Evil willow tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1688;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11437:
                definition.objectModelIDs = new int[] { 45765 };
                definition.anIntArray776 = null;
                definition.name = "Evil oak tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1691;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11435:
                definition.objectModelIDs = new int[] { 45769 };
                definition.anIntArray776 = null;
                definition.name = "Evil tree";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 3;
                definition.sizeY = 3;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 1676;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11433:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 353;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11432:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11431:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 353;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11430:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11429:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 353;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11428:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11427:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 353;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 11426:
                definition.objectModelIDs = new int[] { 45743 };
                definition.anIntArray776 = null;
                definition.name = "Evil root";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { "Chop", };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 128;
                definition.modelSizeH = 128;
                definition.modelSizeY = 128;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                break;
            case 650:
                definition.objectModelIDs = new int[] {24003};
                definition.anIntArray776 = null;
                definition.name = "Easter Egg";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { null, };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 256;
                definition.modelSizeH = 256;
                definition.modelSizeY = 256;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                definition.description = "It's the Easter bunny's eggs!".getBytes();
                break;
            case 651:
                definition.objectModelIDs = new int[] {14414};
                definition.anIntArray776 = null;
                definition.name = "Easter Egg";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { null, };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 228;
                definition.modelSizeH = 228;
                definition.modelSizeY = 228;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                definition.description = "It's the Easter bunny's eggs!".getBytes();
                break;
            case 652:
                definition.objectModelIDs = new int[] {24004};
                definition.anIntArray776 = null;
                definition.name = "Easter Egg";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { null, };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 228;
                definition.modelSizeH = 228;
                definition.modelSizeY = 228;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                definition.description = "It's the Easter bunny's eggs!".getBytes();
                break;

            case 653:
                definition.objectModelIDs = new int[] {24002};
                definition.anIntArray776 = null;
                definition.name = "Easter Egg";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { null, };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 228;
                definition.modelSizeH = 228;
                definition.modelSizeY = 228;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                definition.description = "It's the Easter bunny's eggs!".getBytes();
                break;
            case 654:
                definition.objectModelIDs = new int[] {24001};
                definition.anIntArray776 = null;
                definition.name = "Easter Egg";
                definition.modifiedModelColors = new int[] { 0 };
                definition.originalModelColors = new int[] { 1 };
                definition.sizeX = 1;
                definition.sizeY = 1;
                definition.isUnwalkable = true;
                definition.aBoolean757 = true;
                definition.hasActions = true;
                definition.adjustToTerrain = false;
                definition.nonFlatShading = false;
                definition.aBoolean764 = false;
                definition.animationID = 354;
                definition.anInt775 = 16;
                definition.brightness = 15;
                definition.contrast = 0;
                definition.actions = new String[] { null, };
                definition.mapFunctionID = -1;
                definition.mapSceneID = -1;
                definition.aBoolean751 = false;
                definition.aBoolean779 = true;
                definition.modelSizeX = 228;
                definition.modelSizeH = 228;
                definition.modelSizeY = 228;
                definition.plane = 0;
                definition.offsetX = 0;
                definition.offsetH = 0;
                definition.offsetY = 0;
                definition.aBoolean736 = false;
                definition.isSolidObject = false;
                definition.anInt760 = 1;
                definition.varbitIndex = -1;
                definition.configID = -1;
                definition.configObjectIDs = null;
                definition.description = "It's the Easter bunny's eggs!".getBytes();
                break;
        }

    }

}

