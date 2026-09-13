using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models
{
    public enum TicketType
    {
        Issue,
        Idea,
        [Display(Name = "Project Proposal")] ProjectProposal,
        [Display(Name = "Safety Concern")] SafetyConcern,
        Feedback,
        [Display(Name = "System Request")] SystemRequest
    }
    // Issue
    public enum IssueImpactArea { Production, Quality, SLA, Cost, Delivery }
    public enum SolutionCategory { Operational, People, Process, Material, Equipment }


    // Idea
    public enum IdeaScope { Functional, Departmental, BU }
    public enum ImpactType { Improve, Save, Reduce }

    // Project Proposal
    public enum RoiReach { BU, MultiBU, Corporate }

    // Safety Concern
    public enum RiskLevel { Low, Medium, High, Critical }
    public enum SafetyHazard { UnsafeAct, UnsafeCondition, Environmental, Health }

    // Feedback
    public enum FeedbackCategory { SupervisorBehavior, Workload, Communication, Facilities, Policy, HRProcess }
    public enum PreferredResolution { Personal, Departmental, Organizational }
    public enum Confidentiality { Public, Private, Anonymous }
    public enum FeedbackActionType { ActionRequired, FeedbackOnly, AnonymousShare }
}
