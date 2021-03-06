namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Collections; 
	using System.Collections.Generic;
	
    public class Avatar : KBEngine.GameObject
    {
    	public Combat combat = null;
    	
    	public static SkillBox skillbox = new SkillBox();

        public Dictionary<UInt64, Dictionary<string, object>> itemDict = new Dictionary<UInt64, Dictionary<string, object>>();
        public Dictionary<UInt64, Dictionary<string, object>> equipItemDict = new Dictionary<UInt64, Dictionary<string, object>>();

        private UInt64[] itemIndex2Uids = new UInt64[12];
        private UInt64[] equipIndex2Uids = new UInt64[4];

		public Avatar()
		{
		}
		
		public override void __init__()
		{
			combat = new Combat(this);
			
			if(isPlayer())
			{
				Event.registerIn("relive", this, "relive");
				Event.registerIn("useTargetSkill", this, "useTargetSkill");
				Event.registerIn("jump", this, "jump");
				Event.registerIn("updatePlayer", this, "updatePlayer");
                Event.registerIn("onSpellTarget", this, "onSpellTarget");

                Event.registerIn("sendChatMessage", this, "sendChatMessage");
			}			
		}

		public override void onDestroy ()
		{
			if(isPlayer())
			{
				KBEngine.Event.deregisterIn(this);
			}
		}
		
		public void relive(Byte type)
		{
			cellCall("relive", type);
		}
		
		public bool useTargetSkill(Int32 skillID, Int32 targetID)
		{
			Skill skill = SkillBox.inst.get(skillID);
			if(skill == null)
				return false;

			SCEntityObject scobject = new SCEntityObject(targetID);
			if(skill.validCast(this, scobject))
			{
				skill.use(this, scobject);
				return true;
			}

			Dbg.DEBUG_MSG(className + "::useTargetSkill: skillID=" + skillID + ", targetID=" + targetID + ". is failed!");
			return false;
		}
		
		public void jump()
		{
			cellCall("jump");
		}
		
        public virtual void onSpellTarget(int sid, int tid)
        {
            Event.fireOut("otherAvatarOnSpellTarget", new object[] { this, sid, tid });
        }
		public virtual void onJump()
		{
			Dbg.DEBUG_MSG(className + "::onJump: " + id);
			Event.fireOut("otherAvatarOnJump", new object[]{this});
		}
		
		public virtual void updatePlayer(float x, float y, float z, float yaw)
		{
	    	position.x = x;
	    	position.y = y;
	    	position.z = z;
			
	    	direction.z = yaw;
		}

        public void sendChatMessage(string msg)
        {
            object name = getDefinedProperty("name");
            baseCall("sendChatMessage", (string)name + ": " + msg);
        }
        public void ReceiveChatMessage(string msg)
        {
            Event.fireOut("ReceiveChatMessage", msg);
        }
		public virtual void onAddSkill(Int32 skillID)
		{
			Dbg.DEBUG_MSG(className + "::onAddSkill(" + skillID + ")"); 
			Event.fireOut("onAddSkill", new object[]{this});
			
			Skill skill = new Skill();
			skill.id = skillID;
			skill.name = skillID + " ";
			switch(skillID)
			{
				case 1:
					break;
				case 1000101:
					skill.canUseDistMax = 20f;
					break;
				case 2000101:
					skill.canUseDistMax = 20f;
					break;
				case 3000101:
					skill.canUseDistMax = 20f;
					break;
				case 4000101:
					skill.canUseDistMax = 20f;
					break;
				case 5000101:
					skill.canUseDistMax = 20f;
					break;
				case 6000101:
					skill.canUseDistMax = 20f;
					break;
				default:
					break;
			};

			SkillBox.inst.add(skill);
		}
		
		public virtual void onRemoveSkill(Int32 skillID)
		{
			Dbg.DEBUG_MSG(className + "::onRemoveSkill(" + skillID + ")"); 
			Event.fireOut("onRemoveSkill", new object[]{this});
			SkillBox.inst.remove(skillID);
		}
		
		public override void onEnterWorld()
		{
			base.onEnterWorld();

			if(isPlayer())
			{
				Event.fireOut("onAvatarEnterWorld", new object[]{KBEngineApp.app.entity_uuid, id, this});				
				SkillBox.inst.pull();
			}
		}

        public void reqItemList()
        {
            baseCall("reqItemList");
        }
        public void dropRequest(UInt64 itemUUID)
        {
            baseCall("dropRequest", itemUUID);
        }
        public void swapItemRequest(Int32 srcIndex, Int32 dstIndex)
        {
            UInt64 srcUid = itemIndex2Uids[srcIndex];
            UInt64 dstUid = itemIndex2Uids[dstIndex];

            itemIndex2Uids[srcIndex] = dstUid;
            if (dstUid != 0)
                itemDict[dstUid]["itemIndex"] = srcIndex;
            itemIndex2Uids[dstIndex] = srcUid;
            if (srcUid != 0)
                itemDict[srcUid]["itemIndex"] = dstIndex;

            baseCall("swapItemRequest", new object[] { srcIndex, dstIndex });
        }
        public void equipItemRequest(Int32 itemIndex, Int32 equipIndex)
        {
            baseCall("equipItemRequest", new object[] { itemIndex, equipIndex });
        }
        public void useItemRequest(Int32 itemIndex)
        {
            if (!ConsumeLimitCD.instance.isWaiting())
            {
                baseCall("useItemRequest", new object[] { itemIndex });
                ConsumeLimitCD.instance.Start(2);
            }
            else
            {
                //UI_ErrorHint._instance.errorShow("物品使用冷却中");
            }
        }
        //-----------------------response-------------------------
        public void dropItem_re(Int32 itemId, UInt64 itemUUId)
        {
            Int32 itemIndex = (Int32)(itemDict[itemUUId]["itemIndex"]);
            itemDict.Remove(itemUUId);
            itemIndex2Uids[itemIndex] = 0;
            Event.fireOut("dropItem_re", new object[] { itemIndex });
        }
        public void pickUp_re(Dictionary<string, object> itemInfo)
        {
            Event.fireOut("pickUp_re", new object[] { itemInfo });
            itemDict[(UInt64)itemInfo["UUID"]] = itemInfo;
        }
        public void equipItemRequest_re(Dictionary<string, object> itemInfo, Dictionary<string, object> equipItemInfo)
        {
            Event.fireOut("equipItemRequest_re", new object[] { itemInfo, equipItemInfo });
            UInt64 itemUUid = (UInt64)itemInfo["UUID"];
            UInt64 equipItemUUid = (UInt64)equipItemInfo["UUID"];
            if (itemUUid == 0 && equipItemUUid != 0)//带上装备
            {
                equipItemDict[equipItemUUid] = equipItemInfo;
                itemDict.Remove(equipItemUUid);
            }
            else if (itemUUid != 0 && equipItemUUid != 0)//替换装备
            {
                itemDict.Remove(equipItemUUid);
                equipItemDict[equipItemUUid] = equipItemInfo;
                equipItemDict.Remove(itemUUid);
                itemDict[itemUUid] = itemInfo;
            }
            else if (itemUUid != 0 && equipItemUUid == 0)//脱下装备
            {
                equipItemDict.Remove(itemUUid);
                itemDict[itemUUid] = itemInfo;
            }
        }
        public void onReqItemList(Dictionary<string, object> infos, Dictionary<string, object> equipInfos)
        {
            itemDict.Clear();
            List<object> listinfos = (List<object>)infos["values"];
            for (int i = 0; i < listinfos.Count; i++)
            {
                Dictionary<string, object> info = (Dictionary<string, object>)listinfos[i];
                itemDict.Add((UInt64)info["UUID"], info);
                itemIndex2Uids[(Int32)info["itemIndex"]] = (UInt64)info["UUID"];
            }
            equipItemDict.Clear();
            List<object> elistinfos = (List<object>)equipInfos["values"];
            for (int i = 0; i < elistinfos.Count; i++)
            {
                Dictionary<string, object> info = (Dictionary<string, object>)elistinfos[i];
                equipItemDict.Add((UInt64)info["UUID"], info);
                equipIndex2Uids[(Int32)info["itemIndex"]] = (UInt64)info["UUID"];
            }
            // ui event
            //Dictionary<UInt64, Dictionary<string, object>> itemDicttmp = new Dictionary<ulong, Dictionary<string, object>>(itemDict);
            Event.fireOut("onReqItemList", new object[] { itemDict, equipItemDict });
        }
        public void errorInfo(Int32 errorCode)
        {
            Dbg.DEBUG_MSG("errorInfo(" + errorCode + ")");
        }
    }
} 
