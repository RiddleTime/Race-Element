import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-socials',
  standalone: true,
  imports: [],
  templateUrl: './socials.component.html',
  styleUrl: './socials.component.css'
})
export class SocialsComponent implements OnInit {
  SocialItems: ISocialItem[] = [];

  ngOnInit(): void {
    this.SocialItems = [
      { Text: "GitHub", Href: "https://github.com/RiddleTime/Race-Element" },
      { Text: "Discord", Href: "https://discord.gg/26AAEW5mUq" },
      { Text: "X", Href: "https://x.com/Race_Element" },
      { Text: "Sponsor", Href: "https://paypal.me/CompetizioneManager" },
    ];
  }

}

export interface ISocialItem {
  Text: string;
  Href: string;
}
