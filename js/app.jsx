import React from 'react'
import ReactDOM from 'react-dom'
import classNames from 'classnames'
import 'whatwg-fetch'
import Header from './header.jsx'
import ArtistList from './artistlist.jsx'
import DurationPicker from './durationpicker.jsx'
import LoadingIndicator from './loadingindicator.jsx'

const defaultState = {
    user: {},
    artists: {},
    artistOrder: [],
    duration: 'medium-term',
    isLoading: true
}

class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = defaultState;
        this.fetchData = this.fetchData.bind(this);
    }
    fetchData() {
        fetch('/api/artists')
            .then((response) => { response.json(); })
            .then((json) => {
                console.dir(json);
            });
    }
    componentDidMount() {
        this.fetchData();
    }
    render() {
        var orderedArtists = this.state.artistOrder.map(id => this.state.artists[id]);
        var mainClass = classNames({
            'hidden': this.state.isLoading
        });
        return (
            <div className="container">
                <LoadingIndicator display={this.state.isLoading} />
                <div className={mainClass}>
                    <Header name="Ian" />
                    <DurationPicker />
                    <ArtistList artists={orderedArtists} />
                </div>
            </div>
        )
    }
}

ReactDOM.render(
    <App/>, 
    document.getElementById('app')
);