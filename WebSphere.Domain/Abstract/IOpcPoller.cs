using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSphere.Domain.Concrete;
using WebSphere.Domain.Entities;

namespace WebSphere.Domain.Abstract
{
    public class TagValueContainer
    {
        public TagId Tag;
        public float? LastAnalogValue;
        public bool? LastDiscreteValue;
        public string LastValue;
        public bool Imitation;
        public string RealLastValue;
        public DateTime LastLogged;
        public int Quality;
    }
    public class FitValueContainer
    {
        public string Name;
        public int Id;
        public float sumNow;
        public float summlastDay;
        public float summtoDay;
        public DateTime lastDay_Time;
        public float summlast2H;
        public DateTime last2H_Time;
        public float summto2H;
        public DateTime to2H_Time;

        public float lastDay;
        public float toDay;
        public float last2H;
        public float to2H;
    }
    public class TagId
    {
        public int Id;
        public int PollerId;
        public string TagName;

        public override int GetHashCode()
        {
            return PollerId.GetHashCode() ^ TagName.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            TagId p = obj as TagId;
            if ((System.Object)p == null)
            {
                return false;
            }

            return (TagName == p.TagName) && (PollerId == p.PollerId);
        }

        public bool Equals(TagId p)
        {
            if ((object)p == null)
            {
                return false;
            }

            return (TagName == p.TagName) && (PollerId == p.PollerId);
        }
    }
    public class TagValueDto
    {
        public string Tag;
        public TagValueContainer OpcVals;
        public string Json;
    }
    public interface IOpcPoller
    {
        void Init();
        TagValueContainer ReadTag(TagId tag);
        TagValueContainer ReadTag(string tag);
        TagValueContainer ReadTag(int tagId);
        List<TagValueContainer> ReadTags();
        bool WriteTag(TagId tag, string value);
        string OnReadOpcTag(string tag);
        List<Dictionary<string, dynamic>> GetOpcInfo();
        bool Reinicialize(int pollerId);
        bool Connect(int pollerId);
        bool Stop(int pollerId);
        bool TagImit(TagId tag);

    }

}
