package org.necrotic.client.cache.definition;

import org.necrotic.client.Client;
import org.necrotic.client.FrameReader;
import org.necrotic.client.List;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.cache.ondemand.OnDemandFetcher;
import org.necrotic.client.io.ByteBuffer;
import org.necrotic.client.world.Model;

public final class ObjectDefinition {

    static ByteBuffer[] stream;
    static int[][] streamIndices;

    public static void unpackConfig(Archive streamLoader) {
        stream = new ByteBuffer[3];
        streamIndices = new int[3][];

        stream[0] = new ByteBuffer(streamLoader.get("loc.dat"));
        ByteBuffer idxStream = new ByteBuffer(streamLoader.get("loc.idx"));
        int totalObjects = idxStream.readUnsignedShort();
        streamIndices[0] = new int[totalObjects];
        System.out.println("Loaded " + totalObjects + " 525 objects.");
        int i = 2;
        for (int j = 0; j < totalObjects; j++) {
            streamIndices[0][j] = i;
            i += idxStream.readUnsignedShort();
        }

        stream[1] = new ByteBuffer(streamLoader.get("667loc.dat"));
        idxStream = new ByteBuffer(streamLoader.get("667loc.idx"));
        totalObjects = idxStream.readUnsignedShort();
        streamIndices[1] = new int[totalObjects];
        System.out.println("Loaded " + totalObjects + " 667 objects.");
        i = 2;
        for (int j = 0; j < totalObjects; j++) {
            streamIndices[1][j] = i;
            i += idxStream.readUnsignedShort();
        }

        stream[2] = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/loc.dat"));
        idxStream = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/loc.idx"));
        totalObjects = idxStream.readUnsignedShort();
        streamIndices[2] = new int[totalObjects];
        System.out.println("Loaded " + totalObjects + " OSRS objects.");
        i = 2;
        for (int j = 0; j < totalObjects; j++) {
            streamIndices[2][j] = i;
            i += idxStream.readUnsignedShort();
        }

        cache = new ObjectDefinition[20];
        for (int k = 0; k < 20; k++)
            cache[k] = new ObjectDefinition();
    }

    public static ObjectDefinition forID(int id) {
        for (int j = 0; j < 20; j++) {
            if (cache[j].type == id) {
                return cache[j];
            }
        }

        cacheIndex = (cacheIndex + 1) % 20;
        ObjectDefinition definition = cache[cacheIndex];

        if (id < 0) {
            id = 0;
        }

        int index = 0;
        if (id >= 70_000) {
            definition.dataType = DataType.OSRS;
            id -= 70_000;
            index = 2;
        } else if (id >= streamIndices[0].length || ObjectDefinitionCustom.load667(id)) {
            definition.dataType = DataType.STANDARD;
            index = 1;
        } else {
            definition.dataType = DataType.STANDARD;
        }

        if (id >= streamIndices[index].length) {
            id = 0;
        }

        stream[index].position = streamIndices[index][id];
        definition.type = id + (definition.dataType == DataType.OSRS ? 70_000 : 0);
        definition.setDefaults();
        definition.doRealValues(stream[index]);

        if (definition.dataType == DataType.OSRS) {
            if (definition.configObjectIDs != null) {
                for (int idx = 0; idx < definition.configObjectIDs.length; idx++) {
                    definition.configObjectIDs[idx] += 70_000;
                }
            }

            if (definition.animationID != -1) {
                definition.animationID += 30_000;
            }

            if (definition.anIntArray776 != null) {
                for (int i = 0; i < definition.anIntArray776.length; i++) {
                    if (definition.anIntArray776[i] > 39076) {
                        System.err.println("osrs object model out of range: " + definition.anIntArray776[i] + " " + definition.name);
                        definition.anIntArray776[i] = 0;
                    }
                }
            }
        } else {
            ObjectDefinitionCustom.do667(definition);
        }

        if (definition.dataType == DataType.OSRS && ObjectDefinitionCustom.load377(definition.type)) {
            definition.dataType = DataType.STANDARD;
        }

        if (definition.description == null) {
            definition.description = ("It's a " + definition.name + ".").getBytes();
        }

        return definition;
    }

    public static void nullify() {
        mruNodes1 = null;
        mruNodes2 = null;
        streamIndices = null;
        cache = null;
        stream = null;
    }

    public boolean aBoolean736;
    byte brightness;
    int offsetX;
    public String name;
    int modelSizeY;
    private static final Model[] aModelArray741s = new Model[4];
    byte contrast;
    public int sizeX;
    int offsetH;
    public int mapFunctionID;
    int[] originalModelColors;
    int modelSizeX;
    public int configID;
    boolean aBoolean751;
    public static boolean lowDetail;
    public int type;
    public boolean aBoolean757;
    public int mapSceneID;
    public int configObjectIDs[];
    int anInt760;
    public int sizeY;
    public boolean adjustToTerrain;
    public boolean aBoolean764;
    public static Client clientInstance;
    boolean isSolidObject;
    public boolean isUnwalkable;
    public int plane;
    boolean nonFlatShading;
    private static int cacheIndex;
    int modelSizeH;
    int[] objectModelIDs;
    public int varbitIndex;
    public int anInt775;
    int[] anIntArray776;
    public byte description[];
    public boolean hasActions;
    public boolean aBoolean779;
    public static List mruNodes2 = new List(80);
    public int animationID;
    private static ObjectDefinition[] cache;
    int offsetY;
    int[] modifiedModelColors;
    public static List mruNodes1 = new List(600);
    public String actions[];
    public DataType dataType = DataType.STANDARD;
    public int[] retextureToFind;
    public int[] textureToReplace;

    public void method574(OnDemandFetcher class42_sub1) {
        if (objectModelIDs == null) {
            return;
        }
        for (int objectModelID : objectModelIDs) {
            class42_sub1.method560(objectModelID & 0xffff, 0);
        }
    }

    public boolean method577(int i) {
        if (anIntArray776 == null) {
            if (objectModelIDs == null) {
                return true;
            }
            if (i != 10) {
                return true;
            }
            boolean flag1 = true;
            for (int objectModelID : objectModelIDs) {
                flag1 &= Model.method463(objectModelID & 0xffff, dataType);
            }

            return flag1;
        }
        for (int j = 0; j < anIntArray776.length; j++) {
            if (anIntArray776[j] == i) {
                return Model.method463(objectModelIDs[j] & 0xffff, dataType);
            }
        }

        return true;
    }

    public Model renderObject(int i, int j, int k, int l, int i1, int j1, int k1) {
        Model model = getAnimatedModel(i, k1, j);
        if (model == null) {
            return null;
        }
        if (adjustToTerrain || nonFlatShading) {
            model = new Model(adjustToTerrain, nonFlatShading, model);
        }
        if (adjustToTerrain) {
            int l1 = (k + l + i1 + j1) / 4;
            for (int i2 = 0; i2 < model.vertices; i2++) {
                int j2 = model.vertex_position_x[i2];
                int k2 = model.vertex_position_z[i2];   // note: this was verticesZCoordinate but i renamed the model class and it didn't rename this variable
                                                        // so not sure if it's the right one but 99%
                int l2 = k + (l - k) * (j2 + 64) / 128;
                int i3 = j1 + (i1 - j1) * (j2 + 64) / 128;
                int j3 = l2 + (i3 - l2) * (k2 + 64) / 128;
                model.vertex_position_y[i2] += j3 - l1;
            }

            model.method467();
        }
        return model;
    }

    public boolean method579() {
        if (objectModelIDs == null) {
            return true;
        }
        boolean flag1 = true;
        for (int objectModelID : objectModelIDs) {
            flag1 &= Model.method463(objectModelID & 0xffff, dataType);
        }
        return flag1;
    }

    public ObjectDefinition method580() {
        int i = -1;
        if (varbitIndex != -1) {
            VarBit varBit = VarBit.getCache(dataType)[varbitIndex];
            int j = varBit.configId;
            int k = varBit.lsb;
            int l = varBit.msb;
            int i1 = Client.anIntArray1232[l - k];
            // System.out.println("j: " + j + " k: " + k);
            i = clientInstance.variousSettings[j] >> k & i1;
        } else if (configID != -1) {
            i = clientInstance.variousSettings[configID];
        }
        if (i < 0 || i >= configObjectIDs.length || configObjectIDs[i] == -1) {
            return null;
        } else {
            return forID(configObjectIDs[i]);
        }
    }

    private Model getAnimatedModel(int j, int k, int l) {
        Model model = null;
        long l1;
        if (anIntArray776 == null) {
            if (j != 10) {
                return null;
            }
            l1 = (type << 8) + l + ((long) (k + 1) << 32);
            Model model_1 = (Model) mruNodes2.insertFromCache(l1);
            if (model_1 != null) {
                return model_1;
            }
            if (objectModelIDs == null) {
                return null;
            }
            boolean flag1 = aBoolean751 ^ l > 3;
            int k1 = objectModelIDs.length;
            for (int i2 = 0; i2 < k1; i2++) {
                int l2 = objectModelIDs[i2];
                if (flag1) {
                    l2 += 0x10000;
                }
                model = (Model) mruNodes1.insertFromCache(l2);
                if (model == null) {
                    model = Model.fetchModel(l2 & 0xffff, dataType);
                    if (model == null) {
                        return null;
                    }
                    if (flag1) {
                        model.method477();
                    }
                    mruNodes1.removeFromCache(model, l2);
                }
                if (k1 > 1) {
                    aModelArray741s[i2] = model;
                }
            }

            if (k1 > 1) {
                model = new Model(k1, aModelArray741s);
            }
        } else {
            int i1 = -1;
            for (int j1 = 0; j1 < anIntArray776.length; j1++) {
                if (anIntArray776[j1] != j) {
                    continue;
                }
                i1 = j1;
                break;
            }

            if (i1 == -1) {
                return null;
            }
            l1 = (type << 8) + (i1 << 3) + l + ((long) (k + 1) << 32);
            Model model_2 = (Model) mruNodes2.insertFromCache(l1);
            if (model_2 != null) {
                return model_2;
            }
            if(objectModelIDs == null) {
                return null;
            }
            int j2 = objectModelIDs[i1];
            boolean flag3 = aBoolean751 ^ l > 3;
            if (flag3) {
                j2 += 0x10000;
            }
            model = (Model) mruNodes1.insertFromCache(j2);
            if (model == null) {
                model = Model.fetchModel(j2 & 0xffff, dataType);
                if (model == null) {
                    return null;
                }
                if (flag3) {
                    model.method477();
                }
                mruNodes1.removeFromCache(model, j2);
            }
        }
        boolean flag;
        flag = modelSizeX != 128 || modelSizeH != 128 || modelSizeY != 128;
        boolean flag2;
        flag2 = offsetX != 0 || offsetH != 0 || offsetY != 0;
        Model model_3 = new Model(modifiedModelColors == null, FrameReader.isNullFrame(k), l == 0 && k == -1 && !flag && !flag2, model);
        if (k != -1) {
            model_3.createBones();
            model_3.applyTransform(k);
            model_3.face_skin = null;
            model_3.vertex_skin = null;
        }
        while (l-- > 0) {
            model_3.method473();
        }
        if (modifiedModelColors != null) {
            for (int k2 = 0; k2 < modifiedModelColors.length; k2++) {
                model_3.method476(modifiedModelColors[k2], originalModelColors[k2]);
            }

        }
        if (flag) {
            model_3.scaleT(modelSizeX, modelSizeY, modelSizeH);
        }
        if (flag2) {
            model_3.translate(offsetX, offsetH, offsetY);
        }
        model_3.light(64 + brightness, 768 + contrast * 5, -50, -10, -50, !nonFlatShading);
        if (anInt760 == 1) {
            model_3.obj_plane = model_3.modelHeight;
        }
        mruNodes2.removeFromCache(model_3, l1);
        return model_3;
    }

    void doRealValues(ByteBuffer stream) {
        if (dataType == DataType.STANDARD) {
            readValues(stream);
        } else {
            readValuesOsrs(stream);
        }
    }

    void readValues(ByteBuffer stream) {
        int i = -1;
        label0: do {
            int opcode;
            do {
                opcode = stream.readUnsignedByte();
                if (opcode == 0)
                    break label0;
                if (opcode == 1) {
                    int k = stream.readUnsignedByte();
                    if (k > 0)
                        if (objectModelIDs == null || lowDetail) {
                            anIntArray776 = new int[k];
                            objectModelIDs = new int[k];
                            for (int k1 = 0; k1 < k; k1++) {
                                objectModelIDs[k1] = stream.readUnsignedShort();
                                anIntArray776[k1] = stream.readUnsignedByte();
                            }
                        } else {
                            stream.position += k * 3;
                        }
                } else if (opcode == 2)
                    name = stream.readString();
                else if (opcode == 3)
                    description = stream.readBytes();
                else if (opcode == 5) {
                    int l = stream.readUnsignedByte();
                    if (l > 0)
                        if (objectModelIDs == null || lowDetail) {
                            anIntArray776 = null;
                            objectModelIDs = new int[l];
                            for (int l1 = 0; l1 < l; l1++)
                                objectModelIDs[l1] = stream.readUnsignedShort();
                        } else {
                            stream.position += l * 2;
                        }
                } else if (opcode == 14)
                    sizeX = stream.readUnsignedByte();
                else if (opcode == 15)
                    sizeY = stream.readUnsignedByte();
                else if (opcode == 17)
                    isUnwalkable = false;
                else if (opcode == 18)
                    aBoolean757 = false;
                else if (opcode == 19) {
                    i = stream.readUnsignedByte();
                    if (i == 1)
                        hasActions = true;
                } else if (opcode == 21)
                    adjustToTerrain = true;
                else if (opcode == 22)
                    nonFlatShading = false;
                else if (opcode == 23)
                    aBoolean764 = true;
                else if (opcode == 24) {
                    animationID = stream.readUnsignedShort();
                    if (animationID == 65535)
                        animationID = -1;
                } else if (opcode == 28)
                    anInt775 = stream.readUnsignedByte();
                else if (opcode == 29)
                    brightness = stream.readtSignedByte();
                else if (opcode == 39)
                    contrast = stream.readtSignedByte();
                else if (opcode >= 30 && opcode < 39) {
                    if (actions == null)
                        actions = new String[10];
                    actions[opcode - 30] = stream.readString();
                    if (actions[opcode - 30].equalsIgnoreCase("hidden"))
                        actions[opcode - 30] = null;
                } else if (opcode == 40) {
                    int i1 = stream.readUnsignedByte();
                    modifiedModelColors = new int[i1];
                    originalModelColors = new int[i1];
                    for (int i2 = 0; i2 < i1; i2++) {
                        modifiedModelColors[i2] = stream.readUnsignedShort();
                        originalModelColors[i2] = stream.readUnsignedShort();
                    }
                } else if (opcode == 60)
                    mapFunctionID = stream.readUnsignedShort();
                else if (opcode == 62)
                    aBoolean751 = true;
                else if (opcode == 64)
                    aBoolean779 = false;
                else if (opcode == 65)
                    modelSizeX = stream.readUnsignedShort();
                else if (opcode == 66)
                    modelSizeH = stream.readUnsignedShort();
                else if (opcode == 67)
                    modelSizeY = stream.readUnsignedShort();
                else if (opcode == 68)
                    mapSceneID = stream.readUnsignedShort();
                else if (opcode == 69)
                    plane = stream.readUnsignedByte();
                else if (opcode == 70)
                    offsetX = stream.readSignedShort();
                else if (opcode == 71)
                    offsetH = stream.readSignedShort();
                else if (opcode == 72)
                    offsetY = stream.readSignedShort();
                else if (opcode == 73)
                    aBoolean736 = true;
                else if (opcode == 74) {
                    isSolidObject = true;
                } else {
                    if (opcode != 75)
                        continue;
                    anInt760 = stream.readUnsignedByte();
                }
                continue label0;
            } while (opcode != 77);
            varbitIndex = stream.readUnsignedShort();
            if (varbitIndex == 65535)
                varbitIndex = -1;
            configID = stream.readUnsignedShort();
            if (configID == 65535)
                configID = -1;
            int j1 = stream.readUnsignedByte();
            configObjectIDs = new int[j1 + 1];
            for (int j2 = 0; j2 <= j1; j2++) {
                configObjectIDs[j2] = stream.readUnsignedShort();
                if (configObjectIDs[j2] == 65535)
                    configObjectIDs[j2] = -1;
            }

        } while (true);
        if (i == -1) {
            hasActions = objectModelIDs != null && (anIntArray776 == null || anIntArray776[0] == 10);
            if (actions != null)
                hasActions = true;
        }
        if (isSolidObject) {
            isUnwalkable = false;
            aBoolean757 = false;
        }
        if (anInt760 == -1)
            anInt760 = isUnwalkable ? 1 : 0;
    }

    void readValuesOsrs(ByteBuffer stream) {
        int i = -1;
        label0: do {
            int opcode = stream.readUnsignedByte();
            if (opcode == 0)
                break label0;
            if (opcode == 1) {
                int k = stream.readUnsignedByte();
                if (k > 0)
                    if (objectModelIDs == null || lowDetail) {
                        anIntArray776 = new int[k];
                        objectModelIDs = new int[k];
                        for (int k1 = 0; k1 < k; k1++) {
                            objectModelIDs[k1] = stream.readUnsignedShort();
                            anIntArray776[k1] = stream.readUnsignedByte();
                        }
                    } else {
                        stream.position += k * 3;
                    }
            } else if (opcode == 2)
                name = stream.readString();
            else if (opcode == 3)
                description = stream.readBytes();
            else if (opcode == 5) {
                int l = stream.readUnsignedByte();
                if (l > 0)
                    if (objectModelIDs == null || lowDetail) {
                        anIntArray776 = null;
                        objectModelIDs = new int[l];
                        for (int l1 = 0; l1 < l; l1++)
                            objectModelIDs[l1] = stream.readUnsignedShort();
                    } else {
                        stream.position += l * 2;
                    }
            } else if (opcode == 14)
                sizeX = stream.readUnsignedByte();
            else if (opcode == 15)
                sizeY = stream.readUnsignedByte();
            else if (opcode == 17)
                isUnwalkable = false;
            else if (opcode == 18)
                aBoolean757 = false;
            else if (opcode == 19) {
                i = stream.readUnsignedByte();
                if (i == 1)
                    hasActions = true;
            } else if (opcode == 21)
                adjustToTerrain = true;
            else if (opcode == 22)
                nonFlatShading = false;
            else if (opcode == 23)
                aBoolean764 = true;
            else if (opcode == 24) {
                animationID = stream.readUnsignedShort();
                if (animationID == 65535)
                    animationID = -1;
            } else if (opcode == 27) {
                //interact type
            } else if (opcode == 28)
                anInt775 = stream.readUnsignedByte();
            else if (opcode == 29)
                brightness = stream.readtSignedByte();
            else if (opcode == 39)
                contrast = stream.readtSignedByte();
            else if (opcode >= 30 && opcode < 39) {
                if (actions == null)
                    actions = new String[10];
                actions[opcode - 30] = stream.readString();
                if (actions[opcode - 30].equalsIgnoreCase("hidden"))
                    actions[opcode - 30] = null;
            } else if (opcode == 40) {
                int i1 = stream.readUnsignedByte();
                modifiedModelColors = new int[i1];
                originalModelColors = new int[i1];
                for (int i2 = 0; i2 < i1; i2++) {
                    modifiedModelColors[i2] = stream.readUnsignedShort();
                    originalModelColors[i2] = stream.readUnsignedShort();
                }
            } else if (opcode == 41) {
                int length = stream.readUnsignedByte();
                retextureToFind = new int[length];
                textureToReplace = new int[length];
                //System.out.println("opcode 41");
                for (int index = 0; index < length; ++index)
                {
                    retextureToFind[index] = stream.readUnsignedShort();
                    textureToReplace[index] = stream.readUnsignedShort();
                }
            } else if (opcode == 60)
                mapFunctionID = stream.readUnsignedShort();
            else if (opcode == 62)
                aBoolean751 = true;
            else if (opcode == 64)
                aBoolean779 = false;
            else if (opcode == 65)
                modelSizeX = stream.readUnsignedShort();
            else if (opcode == 66)
                modelSizeH = stream.readUnsignedShort();
            else if (opcode == 67)
                modelSizeY = stream.readUnsignedShort();
            else if (opcode == 68)
                mapSceneID = stream.readUnsignedShort();
            else if (opcode == 69)
                plane = stream.readUnsignedByte();
            else if (opcode == 70)
                offsetX = stream.readSignedShort();
            else if (opcode == 71)
                offsetH = stream.readSignedShort();
            else if (opcode == 72)
                offsetY = stream.readSignedShort();
            else if (opcode == 73)
                aBoolean736 = true;
            else if (opcode == 74) {
                isSolidObject = true;
            } else if (opcode == 75) {
                anInt760 = stream.readUnsignedByte();
            } else if (opcode == 77) {
                varbitIndex = stream.readUnsignedShort();
                if (varbitIndex == 65535)
                    varbitIndex = -1;
                configID = stream.readUnsignedShort();
                if (configID == 65535)
                    configID = -1;
                int j1 = stream.readUnsignedByte();
                configObjectIDs = new int[j1 + 1];
                for (int j2 = 0; j2 <= j1; j2++) {
                    configObjectIDs[j2] = stream.readUnsignedShort();
                    if (configObjectIDs[j2] == 65535)
                        configObjectIDs[j2] = -1;
                }
            } else if (opcode == 78)
            {
                stream.readUnsignedShort();
                stream.readUnsignedByte();
            }
            else if (opcode == 79)
            {
                stream.readUnsignedShort();
                stream.readUnsignedShort();
                stream.readUnsignedByte();
                int length = stream.readUnsignedByte();
                for (int index = 0; index < length; ++index)
                {
                    stream.readUnsignedShort();
                }
            }
            else if (opcode == 81)
            {
                stream.readUnsignedByte();
            }
            else if (opcode == 82)
            {
                stream.readUnsignedShort();
            }
            else if (opcode == 92)
            {
                stream.readUnsignedShort();
                stream.readUnsignedShort();
                stream.readUnsignedShort();
                int length = stream.readUnsignedByte();
                for (int index = 0; index <= length; ++index)
                {
                    stream.readUnsignedShort();
                }
            }
            else if (opcode == 249)
            {
                int length = stream.readUnsignedByte();
                for (int k = 0; k < length; k++)
                {
                    boolean isString = stream.readUnsignedByte() == 1;
                    stream.read24BitInt();
                    if (isString)
                    {
                        stream.readString();
                    }
                    else
                    {
                        stream.readInt();
                    }
                }
            } else {
                System.err.println("Unrecognized object opcode: " + opcode);
            }
        } while (true);
        if (i == -1) {
            hasActions = objectModelIDs != null && (anIntArray776 == null || anIntArray776[0] == 10);
            if (actions != null)
                hasActions = true;
        }
        if (isSolidObject) {
            isUnwalkable = false;
            aBoolean757 = false;
        }
        if (anInt760 == -1)
            anInt760 = isUnwalkable ? 1 : 0;
    }

    void setDefaults() {
        objectModelIDs = null;
        anIntArray776 = null;
        name = null;
        description = null;
        modifiedModelColors = null;
        originalModelColors = null;
        sizeX = 1;
        sizeY = 1;
        isUnwalkable = true;
        aBoolean757 = true;
        hasActions = false;
        adjustToTerrain = false;
        nonFlatShading = false;
        aBoolean764 = false;
        animationID = -1;
        anInt775 = 16;
        brightness = 0;
        contrast = 0;
        actions = null;
        mapFunctionID = -1;
        mapSceneID = -1;
        aBoolean751 = false;
        aBoolean779 = true;
        modelSizeX = 128;
        modelSizeH = 128;
        modelSizeY = 128;
        plane = 0;
        offsetX = 0;
        offsetH = 0;
        offsetY = 0;
        aBoolean736 = false;
        isSolidObject = false;
        anInt760 = -1;
        varbitIndex = -1;
        configID = -1;
        configObjectIDs = null;
        retextureToFind = null;
        textureToReplace = null;
    }


}