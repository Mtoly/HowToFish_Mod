# BetterSwimming

Swimming in How to Fish is a jump-mashing contest. Vanilla clamps you to a constant sink and gives you exactly 5 real
strokes before `_jumpForce` gets divided down to nothing, so staying afloat means hammering space forever.

This makes it behave like water. **Hold jump to swim up, hold crouch to dive, let go and you tread water** at the
surface instead of dropping like a rock. Waves lift you properly too, since vanilla tests your height against the flat
water plane and ignores the actual swell.

All of it is configurable, and everything defaults to something reasonable. Drowning damage and its grace period are
left at vanilla values out of the box.

Client side only. Nothing here touches anyone else's movement.

## Installation

Drop `BetterSwimming.dll` in `BepInEx/plugins`.

---

For questions or comments, find me in the Hexium discord or in my own:

<table width="100%">
  <tr>
    <td align="center">
      <a href="https://hexium.gg">
        <img
          src="https://hexium.gg/assets/Logo.png"
          alt="Hexium"
          width="64"/>
      </a>
    </td>

<td align="center">
      <a href="https://discord.gg/pdHgy6Bsng">
        <img
          src="https://i.imgur.com/Xlcbmm9.png"
          alt="Azumatt's Discord"
          width="64"/>
      </a>
    </td>
  </tr>
</table>
