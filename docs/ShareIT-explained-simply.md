# ShareIT, explained simply 🧸

This is the plain‑English story of what we're building, why, and what changes.
No tech words (or when there is one, I explain it right away).

---

## 1. What is this app, really?

Imagine a big company (Elsewedy Electric) with thousands of employees.

Sometimes an employee wants to **tell the company something**:
- "Something is wrong 😟" (a complaint or a problem)
- "I have a cool idea 💡"
- "There's a safety danger ⚠️"
- "I want to propose a big project 📋"
- "Here's some feedback about my workplace 🗣️"
- "Can we fix/improve a computer system? 💻"

**ShareIT is the friendly box where they drop these messages** — and it makes sure the
right person at the company reads each one and does something about it.

Think of it like a **suggestion box, but smart**: it knows who should read each note and
keeps track of whether the note was handled.

---

## 2. What the app was like BEFORE 📦 (the old way)

Before, the box was **one plain box with one plain form**.

Every message — complaint, idea, feedback — was written on the **same blank form**, with
basically just:
- a box to type your message, and
- who/what it was "against."

It worked, but it was clumsy, like using **one form for everything**:

> Imagine a doctor's office where a broken arm, a headache, and a request for a
> vaccination all use the *exact same* form with one blank line that says "write your
> problem here." You'd be missing all the important details for each situation.

That's what the old app did. A **safety danger** and a **project proposal** are totally
different things, but the old form treated them the same. It also couldn't tell you useful
things like "how many safety issues did Factory A have this month?" because it never asked
the right questions in the first place.

---

## 3. What the PowerPoint asked for 🎯 (the new dream)

The PowerPoint from HR said: **stop using one form for everything.** Instead:

**Give each kind of message its OWN form**, with the *right questions* for that kind.
There are **6 kinds**:

| The 6 kinds | Example |
|---|---|
| 🛠️ **Issue** | "Machine on line 3 keeps breaking." |
| 💡 **Idea** | "We could save paper by going digital." |
| 📋 **Project Proposal** | "Let's build a new solar setup — here's the budget." |
| ⚠️ **Safety Concern** | "The fire exit is blocked!" |
| 🗣️ **Employee Feedback** | "My workload has been too heavy." |
| 💻 **System Request** | "Please add X to our HR software." |

Each kind asks for **different details**. For example:
- A **Safety Concern** asks "How risky is this? (Low/High/Critical)"
- A **Project Proposal** asks "How much will it cost? What's the timeline?"
- An **Idea** asks "What would this improve or save?"

The PowerPoint also wanted:
- To always know which **part of the company** (called a **BU — Business Unit**, like a
  mini‑company inside the big one: *ETC, ESC, UMC, Polymers, Cables Accessories, ECMEI*)
  the message came from.
- The **right person** to automatically get each message (safety messages go to the
  **Safety Manager**; people problems go to **HR**).
- Nice **charts** for bosses: how many messages per BU, what kinds, which problems repeat.
- To **keep** idea messages for **5 years** (a company rule).

---

## 4. What's different, in one picture

```
BEFORE  📦                          AFTER  🗄️
─────────                          ─────────
One box, one blank form            One smart help-desk with 6 labeled forms
for everything.                    Each form asks the RIGHT questions.

You wrote: "problem here ___"      Safety form asks: risk level, hazard type...
                                   Project form asks: cost, ROI, timeline...
                                   Idea form asks: what it saves, sustainability...

Couldn't answer "how many          Can show charts: by BU, by kind,
safety issues this month?"         top-10 repeating problems, statuses.

Everyone saw everything.           Right person auto-gets the right message.
```

---

## 5. The plan we agreed on (how we'll build it) 🧩

> ⚠️ Heads‑up: we've **designed** this together, but haven't built it yet. This is the plan.

We made a few key choices (you picked these in our chat):

1. **One main folder + a special page per kind.**
   Every message gets a **main folder** with the shared stuff (who, which BU, department,
   how urgent, the title, the status). Then, depending on the kind, we clip on **one extra
   page** with that kind's special questions (the safety page, the project page, etc.).
   *(The techy name for this is a "schema" — it just means "how we organize the info into
   labeled boxes." More on that below.)*

2. **Rename "Ticket" → "Submission."** We're calling each message a **Submission** because
   that's the word the PowerPoint uses. (In the old app it was called a "Ticket.")

3. **Turn "Company" into "Business Unit (BU)."** The app already had a "Company" list; we're
   repurposing it to be the BU list (ETC, ESC, UMC, …), since that's what HR cares about.

4. **A simple status journey:** every submission moves **New → In Review → Actioned → Closed**,
   like a package: *ordered → being handled → done → filed away.*

5. **Auto‑delivery to the right person:** safety → Safety Manager, people stuff → HR, others →
   the BU's admin. We're adding two new "job badges" (roles): **HRBP** and **HSE Manager**.

6. **Start fresh with the data.** The current app only has a few pretend/demo records, so we'll
   clear those and put in new demo data that fits the new forms.

---

## 6. The "new schema," explained with boxes 🗃️

**"Schema" just means: the labeled boxes we store information in.** Here they are:

**The main box — `Submission`** (every message gets one):
> a tracking number, the kind, a title, the message, which **BU** + **department**, how
> **urgent**, who it's assigned to, the **status**, and (optional) the person's name/email
> — or "anonymous" if they don't want to say.

**Six special boxes — one gets attached depending on the kind:**
| Kind | Its special box asks about… |
|---|---|
| Issue | impact area, root cause, proposed fix |
| Idea | what it improves/saves, scope, sustainability |
| Project Proposal | cost, benefits, ROI, timeline, stakeholders |
| Safety Concern | risk level, hazard type, suggested action |
| Feedback | category, how private, what resolution they want |
| System Request | *(still deciding the exact questions — see below)* |

**Helper lists (dropdowns):**
- **Business Units** — ETC, ESC, UMC, Polymers, Cables Accessories, ECMEI
- **Departments** — HR, Finance, IT, Operations, …
- **Categories** — the little sub‑labels inside each kind (e.g. Safety → "Unsafe Act,"
  "Unsafe Condition"; Feedback → "Workload," "Communication").

**Plus the helpful extras:** file attachments 📎, a comment thread for the case 💬, and a
history log 📜 (who did what, when).

That's the whole thing! One main box, the right special box clipped on, and some dropdown
lists to keep everything tidy.

---

## 7. Two tiny things we still need to decide 🤔

1. **The "System Request" form** — the PowerPoint never listed its questions. My guess is:
   *"Which system? What's annoying about it now? What change do you want? What would it help?"*
   → Is that okay, or do you have the real questions?

2. **Anything to keep?** I planned to remove a couple of old leftovers (like the "Relation"
   list that let non‑employees submit), because the new plan is **employees‑only**.
   → Okay to remove, or do outsiders need to submit too?

Once you're happy with those two, we lock the design and I write the step‑by‑step build plan.
