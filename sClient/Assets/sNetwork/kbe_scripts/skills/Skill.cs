namespace KBEngine
{
  	using UnityEngine; 
	using System; 
	using System.Collections; 
	using System.Collections.Generic;
	
    public class Skill 
    {
    	public string name;
    	public string descr;
    	public Int32 id;
    	public float canUseDistMin = 0f;
    	public float canUseDistMax = 30f;
    	
		public Skill()
		{
		}
		
		public bool validCast(KBEngine.Entity caster, SCObject target)
		{
			float dist = Vector3.Distance(target.getPosition(), caster.position);
            Debug.Log("skill dis:" + target.getPosition()+" - " + caster.position + " - " + dist + " - " + canUseDistMax);
			if(dist > canUseDistMax)
				return false;
			
			return true;
		}
		
		public void use(KBEngine.Entity caster, SCObject target)
		{
			caster.cellCall("useTargetSkill", id, ((SCEntityObject)target).targetID);
		}
    }
} 
