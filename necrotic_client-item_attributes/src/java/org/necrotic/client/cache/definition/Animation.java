package org.necrotic.client.cache.definition;

import org.necrotic.client.FrameReader;
import org.necrotic.client.Signlink;
import org.necrotic.client.cache.Archive;
import org.necrotic.client.cache.DataType;
import org.necrotic.client.io.ByteBuffer;

public final class Animation {

	public static void unpackConfig(Archive streamLoader)
	{
		ByteBuffer stream1 = new ByteBuffer(streamLoader.get("seq.dat"));
		ByteBuffer stream2 = new ByteBuffer(Archive.ReadFile(Signlink.getCacheDirectory() + "/osrs/seq.dat"));
		int length1 = stream1.readUnsignedShort();
		int length2 = stream2.readUnsignedShort();

		if(cache == null) {
			cache = new Animation[length1 + length2 + 30_000];
		}

		for(int j = 0; j < length1; j++)
		{
			if(cache[j] == null) {
				cache[j] = new Animation();
			}
			cache[j].dataType = DataType.OSRS;
			cache[j].readValues(stream1);
			AnimationCustom.do667(cache, j);
		}

		for(int j = 0; j < length2; j++)
		{
			int index = j + 30_000;
			if(cache[index] == null) {
				cache[index] = new Animation();
			}
			cache[index].dataType = DataType.OSRS;
			cache[index].readValuesOsrs(stream2);

			for (int frameIndex = 0; frameIndex < cache[index].frameIDs.length; frameIndex++) {
				if (cache[index].frameIDs[frameIndex] > -1) {
					cache[index].frameIDs[frameIndex] += 10_000;
				}
			}

			for (int frameIndex = 0; frameIndex < cache[index].frameIDs2.length; frameIndex++) {
				if (cache[index].frameIDs2[frameIndex] > -1) {
					cache[index].frameIDs2[frameIndex] += 10_000;
				}
			}
		}

		/*for (int index = 0; index < cache.length; index++) {
			if (cache[index] == null) {
				cache[index] = new Animation();
				cache[index].frameCount = 1;
				cache[index].frameIDs = new int[1];
				cache[index].frameIDs[0] = -1;
				cache[index].frameIDs2 = new int[1];
				cache[index].frameIDs2[0] = -1;
				cache[index].delays = new int[1];
				cache[index].delays[0] = -1;
			}
		}*/

		System.out.println("Loaded " + length1 + " 667 animations.");
		System.out.println("Loaded " + length2 + " OSRS animations.");
	}

	public int getFrameLength(int i)
	{
		if(i > delays.length)
			return 1;
		int j = delays[i];
		if(j == 0)
		{
			FrameReader reader = FrameReader.forId(frameIDs[i]);
			if(reader != null)
				j = delays[i] = reader.displayLength;
		}
		if(j == 0)
			j = 1;
		return j;
	}



	public void readValues(ByteBuffer stream)
	{
		do {
			int i = stream.readUnsignedByte();
			if(i == 0)
				break;
			if(i == 1) {
				frameCount = stream.readUnsignedShort();
				frameIDs = new int[frameCount];
				frameIDs2 = new int[frameCount];
				delays = new int[frameCount];

				for(int i_ = 0; i_ < frameCount; i_++){
					frameIDs[i_] = stream.readInt();
					frameIDs2[i_] = -1;
				}
				for(int i_ = 0; i_ < frameCount; i_++)
					delays[i_] = stream.readUnsignedByte();
			}
			else if(i == 2)
				loopDelay = stream.readUnsignedShort();
			else if(i == 3) {
				int k = stream.readUnsignedByte();
				animationFlowControl = new int[k + 1];
				for(int l = 0; l < k; l++)
					animationFlowControl[l] = stream.readUnsignedByte();
				animationFlowControl[k] = 0x98967f;
			}
			else if(i == 4)
				oneSquareAnimation = true;
			else if(i == 5)
				forcedPriority = stream.readUnsignedByte();
			else if(i == 6)
				leftHandItem = stream.readUnsignedShort();
			else if(i == 7)
				rightHandItem = stream.readUnsignedShort();
			else if(i == 8)
				frameStep = stream.readUnsignedByte();
			else if(i == 9)
				resetWhenWalk = stream.readUnsignedByte();
			else if(i == 10)
				priority = stream.readUnsignedByte();
			else if(i == 11)
				delayType = stream.readUnsignedByte();

			else if (i == 13) {
				int len = stream.readUnsignedByte();

				for (int k = 0; k < len; k++) {
					stream.read24BitInt();
				}
			}
			else
				System.out.println("Unrecognized seq.dat config code: "+i);
		} while(true);
		if(frameCount == 0)
		{
			frameCount = 1;
			frameIDs = new int[1];
			frameIDs[0] = -1;
			frameIDs2 = new int[1];
			frameIDs2[0] = -1;
			delays = new int[1];
			delays[0] = -1;
		}
		if(resetWhenWalk == -1)
			if(animationFlowControl != null)
				resetWhenWalk = 2;
			else
				resetWhenWalk = 0;
		if(priority == -1)
		{
			if(animationFlowControl != null)
			{
				priority = 2;
				return;
			}
			priority = 0;
		}
	}

	public void readValuesOsrs(ByteBuffer stream)
	{
		do {
			int i = stream.readUnsignedByte();
			if(i == 0)
				break;
			if(i == 1) {
				frameCount = stream.readUnsignedShort();
				frameIDs = new int[frameCount];
				frameIDs2 = new int[frameCount];
				delays = new int[frameCount];

				for(int i_ = 0; i_ < frameCount; i_++)
					delays[i_] = stream.readUnsignedShort();


				for(int i_ = 0; i_ < frameCount; i_++){
					frameIDs[i_] = stream.readUnsignedShort();
					frameIDs2[i_] = -1;
				}

				for(int i_ = 0; i_ < frameCount; i_++){
					frameIDs[i_] += stream.readUnsignedShort() << 16;
					frameIDs2[i_] = -1;
				}

			}
			else if(i == 2)
				loopDelay = stream.readUnsignedShort();
			else if(i == 3) {
				int k = stream.readUnsignedByte();
				animationFlowControl = new int[k + 1];
				for(int l = 0; l < k; l++)
					animationFlowControl[l] = stream.readUnsignedByte();
				animationFlowControl[k] = 0x98967f;
			}
			else if(i == 4)
				oneSquareAnimation = true;
			else if(i == 5)
				forcedPriority = stream.readUnsignedByte();
			else if(i == 6)
				leftHandItem = stream.readUnsignedShort();
			else if(i == 7)
				rightHandItem = stream.readUnsignedShort();
			else if(i == 8)
				frameStep = stream.readUnsignedByte();
			else if(i == 9)
				resetWhenWalk = stream.readUnsignedByte();
			else if(i == 10)
				priority = stream.readUnsignedByte();
			else if(i == 11)
				delayType = stream.readUnsignedByte();
			else if (i == 12)
			{
				int var3 = stream.readUnsignedByte();

				for (int var4 = 0; var4 < var3; ++var4)
				{
					stream.readUnsignedShort();
				}

				for (int var4 = 0; var4 < var3; ++var4)
				{
					stream.readUnsignedShort();
				}
			}
			else if (i == 13) {
				int len = stream.readUnsignedByte();

				for (int k = 0; k < len; k++) {
					stream.read24BitInt();
				}
			}
			//else
				//System.out.println("Unrecognized seq.dat config code: "+i);
		} while(true);
		if(frameCount == 0)
		{
			frameCount = 1;
			frameIDs = new int[1];
			frameIDs[0] = -1;
			frameIDs2 = new int[1];
			frameIDs2[0] = -1;
			delays = new int[1];
			delays[0] = -1;
		}
		if(resetWhenWalk == -1)
			if(animationFlowControl != null)
				resetWhenWalk = 2;
			else
				resetWhenWalk = 0;
		if(priority == -1)
		{
			if(animationFlowControl != null)
			{
				priority = 2;
				return;
			}
			priority = 0;
		}
	}

	Animation()
	{
		loopDelay = -1;
		oneSquareAnimation = false;
		forcedPriority = 5;
		leftHandItem = -1;
		rightHandItem = -1;
		frameStep = 99;
		resetWhenWalk = -1;
		priority = -1;
		delayType = 2;
	}

	public static void nullLoader() {
		cache = null;
	}

	public DataType dataType;
	public static Animation cache[];
	public int frameCount;
	public int frameIDs[];
	public int frameIDs2[];
	public int[] delays;
	public int loopDelay;
	public int animationFlowControl[];
	public boolean oneSquareAnimation;
	public int forcedPriority;
	public int leftHandItem;
	public int rightHandItem;
	public int frameStep;
	public int resetWhenWalk;
	public int priority;
	public int delayType;
	public static int anInt367;
}