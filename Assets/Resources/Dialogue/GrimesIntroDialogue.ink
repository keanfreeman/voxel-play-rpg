-> start_knot

VAR join_party_request = "none"

== start_knot ==
Hey, Kean. -> loop_knot

== loop_knot ==
* [So, your name is Grimes? Is that like the famous...?]
    No. -> loop_knot
* [Got any hobbies?] -> hobby_tangent
* [We could use your help saving that guy in the mines.]
    ~join_party_request = "RogueGrimes"
    And boy howdy. -> END
== hobby_tangent ==
Someone's life is possibly in peril, and you want to know if I have hobbies?
+ [Yes.]
    Well, no. -> loop_knot
+ [No.]
    Thought so. -> loop_knot
