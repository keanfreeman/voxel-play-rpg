-> start_knot

VAR join_party_request = "none"

== start_knot ==
Kean, my good friend! Feeling okay, I hope?
+ [Yea, ready to kick some ass!]
    Glad to hear it! I need you at your best. -> loop_knot
+ [Not so much. I'm feeling a little disoriented.]
    Understandable given the circumstances. But chin up, okay? Everything is easier with practice. -> loop_knot
    
== loop_knot ==
* [Before we go - what was Bard college like?]
    To be honest, I had no idea what I was in for and didn't really gain my stride until the end.
    But it was worth it. Now my free-solos hold others in a trance! The magic is a nice perk, too. -> loop_knot
* [One more thing - what would you say you do around here?]
    Talk with customers, mostly. Nobody else wants to do it. This coal isn't going to sell itself, you know?
    Don't get the wrong idea though, I pitch in in other ways, too. -> loop_knot
* [Let's go find our friend.]
    ~join_party_request = "BardDrill"
    Yes, let's wait no longer. -> END
