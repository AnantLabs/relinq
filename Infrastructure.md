Under construction:
  * Client-side: a singleton app-wide context, unclear how it gets created and initializes its beans, everything that implements the IBean marker is remote.
  * Server-side: a singleton app-wide context, unclear how it gets created and initialized (namespace import), and creates its beans, unclear how it's looked up.
  * Error reporting: full info, or custom exceptions.
  * How to specify preprocessors pipeline.