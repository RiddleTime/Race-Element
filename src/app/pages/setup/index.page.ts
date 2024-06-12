import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  standalone: true,
  template: `
  <div>
    <h2>Hello Analog</h2>

    Analog is a meta-framework on top of Angular.
    </div>
  `,
  imports: []
})
export default class SetupLinkPage implements OnInit {

  // http://192.168.0.130:8080/setup?234234

  setupLink: string = '';
  constructor(private route: ActivatedRoute) {

  }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      let link = params['link'];
      if (link !== undefined) {
        this.setupLink = link;
        console.log(this.setupLink);
        window.location.assign('RaceElement://setup=' + this.setupLink);
      }
    });
  }
}
