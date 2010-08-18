// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Scope = "member", Target = "Essential.StringTemplate+GetValue.#Invoke(System.String,System.Object&)", Justification = "Required for compatibility with IDictionary.TryGetValue, i.e. require separate return values for success and the value.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate", Scope = "member", Target = "Essential.StringTemplate+GetValue.#Invoke(System.String,System.Object&)", Justification = "Required for compatibility with IDictionary.TryGetValue.")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Essential.Data")]
