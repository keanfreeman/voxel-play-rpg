-> start_knot

VAR join_party_request = "none"
VAR play_sound_effect = "none"

== start_knot == 
It's your cat, Soot.
-> loop_knot

== loop_knot ==
+ [Pet Soot.]
    ~ play_sound_effect = "meow"
    Soot enjoyed that!
    -> loop_knot
+ [Take Soot along with you.]
    You took Soot along with you!
    ~ join_party_request = "Cat"
    -> END
+ [Leave.] -> END
